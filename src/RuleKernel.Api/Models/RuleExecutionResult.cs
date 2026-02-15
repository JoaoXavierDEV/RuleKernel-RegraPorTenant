namespace RuleKernel.Models;

public class RuleExecutionResult
{
    public Guid RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public object? Result { get; set; }
    public string? ErrorMessage { get; set; }
}
