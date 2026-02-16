using RuleKernel.Core.Contract;
using RuleKernel.Core.Services;

namespace RuleKernel.Api.Services;

public sealed class CalcularService
{
    private readonly IRuleRunner _ruleRunner;

    public CalcularService(IRuleRunner ruleRunner)
    {
        _ruleRunner = ruleRunner;
    }

    public async Task<DateTime> CalcularDataDeVencimentoAsync(CancellationToken cancellationToken = default)
    {
        var contrato = new DataDeVencimentoContract
        {
            InDataDeEmissao = DateTime.Now
        };         

        await _ruleRunner.ExecutarRegra("SALOME_DataDeVencimento", contrato, cancellationToken);

        Console.WriteLine(contrato.OutResult);

        return contrato.OutResult;
    }
}
