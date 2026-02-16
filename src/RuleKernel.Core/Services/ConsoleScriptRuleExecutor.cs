using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace RuleKernel.Core.Services;


public sealed class ConsoleScriptRuleExecutor
{
    public Task ExecuteAsync(string source, object? contract, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("O código-fonte da regra não pode ser vazio.", nameof(source));
        }

        if (contract is null)
        {
            throw new ArgumentNullException(nameof(contract), "Toda regra deve ter um contract.");
        }

        return ExecuteSourceAsync(source, contract, cancellationToken);
    }

    private async Task ExecuteSourceAsync(string source, object? contract, CancellationToken cancellationToken)
    {
        await Task.Yield();

        var globalsType = contract?.GetType() is { } t
            ? typeof(ScriptGlobals<>).MakeGenericType(t)
            : typeof(ScriptGlobals<object?>);

        var globals = Activator.CreateInstance(globalsType, new RegraBaseHost(contract), contract);
        var scriptOptions = CreateScriptOptions();

        var script = CSharpScript.Create(source, options: scriptOptions, globalsType: globalsType);
        var compilationDiagnostics = script.Compile(cancellationToken);

        var errors = compilationDiagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.ToString())
            .ToList();

        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Falha ao compilar regra C#: {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        await script.RunAsync(globals, cancellationToken);
    }

    private static ScriptOptions CreateScriptOptions()
    {
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location));

        return ScriptOptions.Default
            .WithOptimizationLevel(OptimizationLevel.Release)
            .WithReferences(references)
            .WithImports(
                "System",
                "System.Threading",
                "System.Threading.Tasks",
                "RuleKernel.Core.Services",
                "RuleKernel.Core.Contract");
    }

    public sealed class RegraBaseHost : RegraBase
    {
        private static readonly AsyncLocal<CancellationToken> AmbientCancellationToken = new();

        public RegraBaseHost(object? contract)
        {
            Contract = contract;
        }

        public static void SetAmbientCancellationToken(CancellationToken cancellationToken)
            => AmbientCancellationToken.Value = cancellationToken;

        public CancellationToken CancellationToken => AmbientCancellationToken.Value;

        public object? Contract { get; }
    }
}