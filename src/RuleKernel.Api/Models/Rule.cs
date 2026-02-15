namespace RuleKernel.Models;

public class Rule
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RuleDefinitionId { get; set; }
    public string SourceCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public RuleDefinition RuleDefinition { get; set; } = null!;
}
