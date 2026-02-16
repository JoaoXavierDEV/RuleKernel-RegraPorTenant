using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Contract;
using RuleKernel.Core.Data;

namespace RuleKernel.Core.Services;

public interface IRuleRunner
{
    Task ExecutarRegra<TContract>(string ruleName, TContract contract, CancellationToken cancellationToken = default)
        where TContract : class;

    Task ExecutarRegra(string ruleName, object contract, CancellationToken cancellationToken = default);
}

public sealed class RuleRunner : IRuleRunner
{
    private readonly RuleKernelDbContext _db;
    private readonly ConsoleScriptRuleExecutor _executor;

    public RuleRunner(RuleKernelDbContext db, ConsoleScriptRuleExecutor executor)
    {
        _db = db;
        _executor = executor;
    }

    public Task ExecutarRegra<TContract>(string ruleName, TContract contract, CancellationToken cancellationToken = default)
        where TContract : class
        => ExecutarRegra(ruleName, contract, typeof(TContract), cancellationToken);

    public Task ExecutarRegra(string ruleName, object contract, CancellationToken cancellationToken = default)
    {
        if (contract is null) throw new ArgumentNullException(nameof(contract));
        return ExecutarRegra(ruleName, contract, contract.GetType(), cancellationToken);
    }

    private async Task ExecutarRegra(string ruleName, object contract, Type contractType, CancellationToken cancellationToken)
    {
        var contractTypeName = contractType.FullName ?? contractType.Name;

        var definition = await _db.RuleDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Name == ruleName && d.ContractType == contractTypeName, cancellationToken);

        if (definition is null)
        {
            throw new InvalidOperationException($"Tipo de regra não encontrado para '{ruleName}' com contrato '{contractTypeName}'.");
        }

        var rules = await _db.Rules
            .AsNoTracking()
            .Where(r => r.IsActive && r.RuleDefinitionId == definition.Id)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);

        if (rules.Count == 0)
        {
            throw new InvalidOperationException($"Nenhuma regra ativa encontrada para '{ruleName}' com contrato '{contractTypeName}'.");
        }

        foreach (var rule in rules)
        {
            await _executor.ExecuteAsync(rule.SourceCode, contract, cancellationToken);
        }
    }
}
