using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Data;
using RuleKernel.Core.Models;
using RuleKernel.Core.Services;

namespace RuleKernel.Api.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/faturas")]
public sealed class FaturasController : ControllerBase
{
    private readonly RuleKernelDbContext _db;
    private readonly FaturaService _service;

    public FaturasController(RuleKernelDbContext db, FaturaService service)
    {
        _db = db;
        _service = service;
    }

    [HttpPost("emitir")]
    public async Task<ActionResult<Fatura>> Emitir(
        Guid tenantId,
        EmitirFaturaRequest request,
        CancellationToken cancellationToken)
    {
        var tenantExists = await _db.Tenants.AnyAsync(t => t.Id == tenantId && t.IsActive, cancellationToken);
        if (!tenantExists) return NotFound("Tenant não encontrado/ativo");

        var fatura = await _service.EmitirFaturaAsync(
            tenantId: tenantId,
            valorPrincipal: request.ValorPrincipal,
            dataDeCredito: request.DataDeCredito,
            dataDeEmissao: request.DataDeEmissao,
            cancellationToken: cancellationToken);

        return Ok(fatura);
    }
}

public sealed record EmitirFaturaRequest(decimal ValorPrincipal, DateTime DataDeCredito, DateTime? DataDeEmissao);
