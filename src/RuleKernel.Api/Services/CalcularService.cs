using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Contract;
using RuleKernel.Core.Data;
using RuleKernel.Core.Services;

namespace RuleKernel.Api.Services;

public sealed class CalcularService
{
    private readonly RuleKernelDbContext _db;
    private readonly IRuleRunner _ruleRunner;

    public CalcularService(RuleKernelDbContext db, IRuleRunner ruleRunner)
    {
        _db = db;
        _ruleRunner = ruleRunner;
    }

    public async Task<DateTime> CalcularDataDeVencimentoAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var usuario = await _db.Tenants
            .Include(t => t.RegraDataVencimento)!
                .ThenInclude(r => r!.RuleDefinition)
            .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive, cancellationToken);

        if (usuario is null)
            throw new InvalidOperationException($"Tenant não encontrado/ativo: '{tenantId}'.");

        if (usuario.RegraDataVencimento?.RuleDefinition?.Name is not null)
            throw new InvalidOperationException("Tenant sem RegraDataVencimento associada.");

        var contrato = new DataDeVencimentoContract
        {
            InDataDeEmissao = DateTime.Now
        };         

        await _ruleRunner.ExecutarRegra(usuario.RegraDataVencimento!.RuleDefinition!.Name, contrato, cancellationToken);

        Console.WriteLine(contrato.OutResult);

        return contrato.OutResult;
    }
}
