namespace RuleKernel.Core.Models;

public sealed class RuleDefinition
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ContractType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
