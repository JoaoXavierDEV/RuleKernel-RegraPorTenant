namespace RuleKernel.Core.Models;

public sealed class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public Guid? RegraDataVencimentoId { get; set; }
    public Rule? RegraDataVencimento { get; set; } 

    public Guid? RegraCalculoDescontoId { get; set; }
    public Rule? RegraCalculoDesconto { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Fatura> Faturas { get; set; } = new List<Fatura>();
}
