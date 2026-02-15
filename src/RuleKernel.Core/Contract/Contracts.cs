namespace RuleKernel.Core.Contract;

public interface IRuleContract<TResult>
{
    bool OutErro { get; set; }
    string? OutMensagem { get; set; }
    TResult? OutResult { get; set; }
}

public sealed class DataDeVencimentoContract
    : IRuleContract<DateTime>
{
    public required DateTime InDataDeCredito { get; init; }
    public DateTime OutDataVencimento { get; set; }

    public bool OutErro { get; set; }
    public string? OutMensagem { get; set; }
    public DateTime OutResult { get; set; }
}

public sealed class TaxaDeJurosContract
    : IRuleContract<decimal>
{
    public required decimal InValorPrincipal { get; init; }
    public required DateTime InDataDeCredito { get; init; }
    public decimal OutValorComJuros { get; set; }

    public bool OutErro { get; set; }
    public string? OutMensagem { get; set; }
    public decimal OutResult { get; set; }
}
