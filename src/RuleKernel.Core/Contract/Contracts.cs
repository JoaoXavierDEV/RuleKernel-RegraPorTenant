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
    public DateTime InDataDeEmissao { get; init; }
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

public sealed class CalculoDescontoContract
    : IRuleContract<decimal>
{
    public required Guid InTenantId { get; init; }
    public required Guid InFaturaId { get; init; }

    public required DateTime InDataDeEmissao { get; init; }

    public required decimal InValorPrincipal { get; init; }
    public decimal InPercentualTaxaAdministracao { get; init; }
    public decimal InPercentualDesconto { get; init; }

    public decimal OutDesconto { get; set; }

    public decimal OutValorTotal { get; set; }

    public bool OutErro { get; set; }
    public string? OutMensagem { get; set; }
    public decimal OutResult { get; set; }
}

