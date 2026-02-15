namespace RuleKernel.Services;

public sealed class CalcularService
{
    private readonly IRuleRunner _ruleRunner;

    public CalcularService(IRuleRunner ruleRunner)
    {
        _ruleRunner = ruleRunner;
    }

    public async Task<DateTime> CalcularDataDeVencimentoAsync(CancellationToken cancellationToken = default)
    {
        var contrato = new RuleKernel.Contract.DataDeVencimentoContract
        {
            InDataDeCredito = DateTime.Now
        };
        //contrato.OutResult = contrato.InDataDeCredito.AddDays(7);
        /*
         corpo da regra

        
        */
         

        await _ruleRunner.ExecutarRegra("SLM_DataVencimento", contrato, cancellationToken);

        Console.WriteLine(contrato.OutResult);

        return contrato.OutResult;
    }
}
