namespace RuleKernel.Core.Models;

public sealed class Rule
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public Guid RuleDefinitionId { get; set; }
    public RuleDefinition? RuleDefinition { get; set; }

    public string SourceCode { get; set; } = string.Empty;

    public int Priority { get; set; }

    public bool IsActive { get; set; } = true;
}
