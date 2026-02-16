namespace RuleKernel.Core.Contract;

public interface IRuleContract
{
    bool OutErro { get; set; }
    string? OutMensagem { get; set; }
}

public interface ICalculoDescontoContract<TResult> : IRuleContract where TResult : struct
{
    TResult OutResult { get; set; }

}

public sealed class DataDeVencimentoContract : IRuleContract
{
    public DateTime InDataDeEmissao { get; init; }
    public DateTime OutDataVencimento { get; set; }

    public bool OutErro { get; set; }
    public string? OutMensagem { get; set; }
}


public sealed class CalculoDescontoContract : IRuleContract
{
    public required Guid InTenantId { get; init; }
    public required Guid InFaturaId { get; init; }
    public bool OutErro { get; set; }
    public string? OutMensagem { get; set; }


    public required DateTime InDataDeEmissao { get; init; }
    public required decimal InValorPrincipal { get; init; }
    public DateTime InDataVencimento { get; set; }





    public decimal OutPercentualDesconto { get; set; }

    public decimal OutResult { get; set; }
}

