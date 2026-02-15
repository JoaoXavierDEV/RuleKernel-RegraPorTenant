using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RuleKernel.Services;

public interface IConsoleRule
{
    Task ExecuteAsync(string tenant, CancellationToken cancellationToken = default);
}

public sealed class ConsoleScriptRuleExecutor
{
    private static readonly string DefaultRuleSource = """
using System;
using System.Threading;
using System.Threading.Tasks;
using RuleKernel.Services;

public sealed class HelloWorldRule : RegraBase, IConsoleRule
{
    public Task ExecuteAsync(string tenant, CancellationToken cancellationToken = default)
    {
        Imprimir($"Olá mundo {tenant}");
        return Task.CompletedTask;
    }
}
""";

    public async Task ExecuteAsync(string tenant, string? csharpSource = null, CancellationToken cancellationToken = default)
    {
        var source = string.IsNullOrWhiteSpace(csharpSource) ? DefaultRuleSource : csharpSource;

        var assembly = await CompileAsync(source, cancellationToken);
        var ruleType = assembly.GetTypes().FirstOrDefault(t =>
            typeof(IConsoleRule).IsAssignableFrom(t) &&
            typeof(RegraBase).IsAssignableFrom(t) &&
            !t.IsAbstract);

        if (ruleType is null)
        {
            throw new InvalidOperationException("Nenhuma implementação de IConsoleRule encontrada no código.");
        }

        var rule = (IConsoleRule)Activator.CreateInstance(ruleType)!;
        await rule.ExecuteAsync(tenant, cancellationToken);
    }

    private static async Task<Assembly> CompileAsync(string source, CancellationToken cancellationToken)
    {
        await Task.Yield();

        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        var compilation = CSharpCompilation.Create(
            assemblyName: $"RuleKernel.DynamicRules.{Guid.NewGuid():N}",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));

        await using var peStream = new MemoryStream();
        var emitResult = compilation.Emit(peStream, cancellationToken: cancellationToken);
        
        if (!emitResult.Success)
        {
            var errors = string.Join(Environment.NewLine, emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));

            throw new InvalidOperationException($"Falha ao compilar regra C#: {Environment.NewLine}{errors}");
        }

        peStream.Position = 0;
        return Assembly.Load(peStream.ToArray());
    }
}
