using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Contract;
using RuleKernel.Core.Data;
using RuleKernel.Core.Models;

namespace RuleKernel.Core.Services;

public sealed class FaturaService
{
    private readonly RuleKernelDbContext _db;
    private readonly IRuleRunner _ruleRunner;

    public FaturaService(RuleKernelDbContext db, IRuleRunner ruleRunner)
    {
        _db = db;
        _ruleRunner = ruleRunner;
    }

    public async Task<Fatura> CalcularAsync(
        Fatura fatura,
        DataDeVencimentoContract vencimentoContract,
        CalculoDescontoContract descontoContract,
        CancellationToken cancellationToken = default)
    {
        if (fatura is null) throw new ArgumentNullException(nameof(fatura));
        if (vencimentoContract is null) throw new ArgumentNullException(nameof(vencimentoContract));
        if (descontoContract is null) throw new ArgumentNullException(nameof(descontoContract));

        var tenant = await _db.Tenants
            .AsNoTracking()
            .Include(t => t.RegraDataVencimento)!.ThenInclude(r => r!.RuleDefinition)
            .Include(t => t.RegraCalculoDesconto)!.ThenInclude(r => r!.RuleDefinition)
            .FirstOrDefaultAsync(t => t.Id == fatura.TenantId && t.IsActive, cancellationToken);

        if (tenant is null) throw new InvalidOperationException($"Tenant não encontrado/ativo: '{fatura.TenantId}'.");        

        if (tenant.RegraDataVencimento is null) throw new InvalidOperationException("Tenant sem RegraDataVencimento associada.");

        if (tenant.RegraCalculoDesconto is null) throw new InvalidOperationException("Tenant sem RegraCalculoDesconto associada.");

        if (tenant.RegraDataVencimento.RuleDefinition is null) throw new InvalidOperationException("RegraDataVencimento sem RuleDefinition carregada.");

        if (tenant.RegraCalculoDesconto.RuleDefinition is null) throw new InvalidOperationException("RegraCalculoDesconto sem RuleDefinition carregada.");


        await _ruleRunner.ExecutarRegra(tenant.RegraDataVencimento.RuleDefinition!.Name, vencimentoContract, cancellationToken);
        fatura.DataDeVencimento = vencimentoContract.OutResult;

        await _ruleRunner.ExecutarRegra(tenant.RegraCalculoDesconto.RuleDefinition!.Name, descontoContract, cancellationToken);
        fatura.Desconto = descontoContract.OutDesconto;
        fatura.ValorTotal = descontoContract.OutValorTotal;
        // TODO verificar diferenças de valores e logar se necessário

        return fatura;
    }

    public async Task<Fatura> EmitirFaturaAsync(
        Guid tenantId,
        decimal valorPrincipal,
        DateTime dataDeCredito,
        DateTime? dataDeEmissao = null,
        CancellationToken cancellationToken = default)
    {
        var faturaId = Guid.NewGuid();

        var fatura = new Fatura
        {
            Id = faturaId,
            TenantId = tenantId,
            ValorPrincipal = valorPrincipal,
            DataDeEmissao = (dataDeEmissao ?? DateTime.UtcNow).Date,
            CreatedAt = DateTime.UtcNow,
        };

        var vencimentoContract = new DataDeVencimentoContract
        {
            InDataDeEmissao = fatura.DataDeEmissao,
        };

        var descontoContract = new CalculoDescontoContract
        {
            InTenantId = fatura.TenantId,
            InFaturaId = fatura.Id,
            InDataDeEmissao = fatura.DataDeEmissao,
            InValorPrincipal = fatura.ValorPrincipal,
            InPercentualTaxaAdministracao = 0m,
            InPercentualDesconto = 0m,
        };

        await CalcularAsync(fatura, vencimentoContract, descontoContract, cancellationToken);

        _db.Faturas.Add(fatura);

        await _db.SaveChangesAsync(cancellationToken);

        return fatura;
    }
}
