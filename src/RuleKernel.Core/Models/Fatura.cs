namespace RuleKernel.Core.Models;

public sealed class Fatura
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public DateTime DataDeEmissao { get; set; }
    public DateTime DataDeVencimento { get; set; }

    public decimal ValorPrincipal { get; set; }
    public decimal TaxaAdministracao { get; set; }
    public decimal Desconto { get; set; }
    public decimal Juros { get; set; }
    public decimal Multa { get; set; }
    public decimal ValorTotal { get; set; }

    public DateTime CreatedAt { get; set; }
}
