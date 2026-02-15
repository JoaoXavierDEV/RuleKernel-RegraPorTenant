namespace RuleKernel.Core.Models;

public sealed class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Rule> Rules { get; set; } = new List<Rule>();
}
