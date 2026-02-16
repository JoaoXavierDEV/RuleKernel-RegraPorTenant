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

    /// <summary>
    /// Emite uma nova fatura para o tenant especificado.
    /// </summary>
    /// <param name="tenantId">O identificador exclusivo do tenant para o qual a fatura será emitida. O tenant deve estar ativo.</param>
    /// <param name="request">Os dados necessários para emissão da fatura, incluindo o valor principal.</param>
    /// <param name="cancellationToken">O token que pode ser usado para cancelar a operação de emissão da fatura.</param>
    /// <returns>Um resultado de ação contendo a fatura emitida, ou um erro caso o tenant não seja encontrado ou não esteja
    /// ativo.</returns>
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
            dataDeEmissao: DateTime.UtcNow.Date,
            cancellationToken: cancellationToken);

        return Ok(fatura);
    }


    /// <summary>
    /// EMITE UMA FATURA A PARTIR DE REGRAS DE VENCIMENTO E DESCONTO ESPECÍFICAS, ATUALIZANDO O TENANT COM ESSAS REGRAS ANTES DA EMISSÃO
    /// </summary>
    /// <param name="tenantId">O identificador exclusivo do tenant para o qual a fatura será emitida. O tenant deve estar ativo.</param>
    /// <param name="request">Os dados necessários para emissão da fatura, incluindo os identificadores das regras de vencimento e desconto e o valor principal.</param>
    /// <param name="cancellationToken">O token que pode ser usado para cancelar a operação de emissão da fatura.</param>
    /// <returns>Um resultado de ação contendo a fatura emitida, ou um erro caso o tenant ou as regras não sejam encontrados ou não estejam ativos.</returns>
    [HttpPost("emitir-por-regras")]
    [Consumes("application/json")]
    public async Task<ActionResult<Fatura>> EmitirPorRegras(
        Guid tenantId,
        EmitirPorRegrasRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive, cancellationToken);
        if (tenant is null) return NotFound("Tenant não encontrado/ativo");

        var vencimentoRule = await _db.Rules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RegraVencimentoId && r.IsActive, cancellationToken);
        if (vencimentoRule is null) return NotFound("Regra de vencimento não encontrada/ativa");

        var descontoRule = await _db.Rules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RegraDescontoId && r.IsActive, cancellationToken);
        if (descontoRule is null) return NotFound("Regra de desconto não encontrada/ativa");

        tenant.RegraDataVencimentoId = vencimentoRule.Id;
        tenant.RegraCalculoDescontoId = descontoRule.Id;

        await _db.SaveChangesAsync(cancellationToken);

        var fatura = await _service.EmitirFaturaAsync(
            tenantId: tenantId,
            valorPrincipal: request.ValorPrincipal,
            dataDeEmissao: DateTime.UtcNow.Date,
            cancellationToken: cancellationToken);

        return Ok(fatura);
    }
}

public sealed record EmitirFaturaRequest(decimal ValorPrincipal);

public sealed record EmitirPorRegrasRequest(Guid RegraVencimentoId, Guid RegraDescontoId, decimal ValorPrincipal);
