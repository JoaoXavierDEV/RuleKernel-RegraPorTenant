using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RuleKernel.Data;
using RuleKernel.Models;
using RuleKernel.Services;

namespace RuleKernel.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/rules-csharp")]
public class CSharpRulesController : ControllerBase
{
    private readonly RuleKernelDbContext _db;
    private readonly ConsoleScriptRuleExecutor _executor;

    public CSharpRulesController(RuleKernelDbContext db, ConsoleScriptRuleExecutor executor)
    {
        _db = db;
        _executor = executor;
    }

    [HttpGet]
    public async Task<ActionResult<List<Rule>>> GetAll(Guid tenantId, CancellationToken cancellationToken)
    {
        return await _db.Rules
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<Rule>> Create(Guid tenantId, CreateCSharpRuleRequest request, CancellationToken cancellationToken)
    {
        var tenantExists = await _db.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken);
        if (!tenantExists) return NotFound("Tenant não encontrado");

        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            SourceCode = request.SourceCode,
            Priority = request.Priority,
            IsActive = request.IsActive
        };

        _db.Rules.Add(rule);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetAll), new { tenantId }, rule);
    }

    [HttpPut("{ruleId:guid}")]
    public async Task<IActionResult> Update(Guid tenantId, Guid ruleId, UpdateCSharpRuleRequest request, CancellationToken cancellationToken)
    {
        var rule = await _db.Rules.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == ruleId, cancellationToken);
        if (rule is null) return NotFound();

        rule.Name = request.Name;
        rule.Description = request.Description;
        rule.SourceCode = request.SourceCode;
        rule.Priority = request.Priority;
        rule.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{ruleId:guid}/execute")]
    public async Task<IActionResult> Execute(Guid tenantId, Guid ruleId, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null) return NotFound("Tenant não encontrado");

        var rule = await _db.Rules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == ruleId && r.IsActive, cancellationToken);

        if (rule is null) return NotFound("Regra não encontrada");

        await _executor.ExecuteAsync(tenant.Name, rule.SourceCode, cancellationToken);
        return Ok(new { Executed = true, ruleId });
    }
}

public record CreateCSharpRuleRequest(string Name, string Description, string SourceCode, int Priority, bool IsActive = true);
public record UpdateCSharpRuleRequest(string Name, string Description, string SourceCode, int Priority, bool IsActive);
