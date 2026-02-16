using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RuleKernel.Api.Services;
using RuleKernel.Core.Data;
using RuleKernel.Core.Models;
using RuleKernel.Core.Services;

namespace RuleKernel.Api.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/rules-csharp")]
public class CSharpRulesController : ControllerBase
{
    private readonly RuleKernelDbContext _db;
    private readonly IRuleRunner _ruleRunner;

    public CSharpRulesController(RuleKernelDbContext db, IRuleRunner ruleRunner)
    {
        _db = db;
        _ruleRunner = ruleRunner;
    }

    [HttpGet]
    public async Task<ActionResult<List<Rule>>> GetAll(Guid tenantId, CancellationToken cancellationToken)
    {
        return await _db.Rules
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .Include(r => r.RuleDefinition)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    [HttpGet("/api/rules-csharp")]
    public async Task<ActionResult<List<Rule>>> GetAllTenants(CancellationToken cancellationToken)
    {
        return await _db.Rules
            .AsNoTracking()
            .Include(r => r.RuleDefinition)
            .OrderBy(r => r.Priority)
            .Select(x => new Rule
            {
                Id = x.Id,
                IsActive = x.IsActive,
                                RuleDefinitionId = x.RuleDefinitionId,
                                Priority = x.Priority,
                                 RuleDefinition = x.RuleDefinition,
                                 SourceCode = x.SourceCode.Replace("                                    ", "").ToString().Replace("\n0m;", "0m;"),
                                 Tenant = x.Tenant,
                                 TenantId = x.TenantId
            })
            .ToListAsync(cancellationToken);
    }

    [HttpGet("/api/rules-csharp-stream")]
    public async IAsyncEnumerable<Rule> GetAllTenantsStream([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var rule in _db.Rules
            .AsNoTracking()
            .Include(r => r.RuleDefinition)
            .OrderBy(r => r.Priority)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken))
        {
            yield return rule;
            await Task.Delay(1000);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Rule>> Create(Guid tenantId, CreateCSharpRuleRequest request, CancellationToken cancellationToken)
    {
        var tenantExists = await _db.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken);
        if (!tenantExists) return NotFound("Tenant não encontrado");

        var definitionExists = await _db.RuleDefinitions.AnyAsync(d => d.Id == request.RuleDefinitionId, cancellationToken);
        if (!definitionExists) return NotFound("Tipo de regra não encontrado");

        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RuleDefinitionId = request.RuleDefinitionId,
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

        var definitionExists = await _db.RuleDefinitions.AnyAsync(d => d.Id == request.RuleDefinitionId, cancellationToken);
        if (!definitionExists) return NotFound("Tipo de regra não encontrado");

        rule.RuleDefinitionId = request.RuleDefinitionId;
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
            .Include(r => r.RuleDefinition)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == ruleId && r.IsActive, cancellationToken);

        if (rule is null) return NotFound("Regra não encontrada");

        if (string.IsNullOrWhiteSpace(rule.RuleDefinition?.Name))
        {
            return Problem("RuleDefinition não carregada para execução.");
        }

        await _ruleRunner.ExecutarRegra(rule.RuleDefinition.Name, new { Tenant = tenant }, cancellationToken);
        return Ok(new { Executed = true, ruleId });
    }

    [HttpPost("{ruleId:guid}/executarRegra")]
    public async Task<IActionResult> ExecuteRegra(Guid tenantId, Guid ruleId, CancellationToken cancellationToken, [FromServices]CalcularService service)
    {
        var result = await service.CalcularDataDeVencimentoAsync(tenantId, cancellationToken);
        return Ok(new { Executed = true, ruleId, Data = result.ToShortDateString() });
    }
}

public record CreateCSharpRuleRequest(Guid RuleDefinitionId, string SourceCode, int Priority, bool IsActive = true);
public record UpdateCSharpRuleRequest(Guid RuleDefinitionId, string SourceCode, int Priority, bool IsActive);
