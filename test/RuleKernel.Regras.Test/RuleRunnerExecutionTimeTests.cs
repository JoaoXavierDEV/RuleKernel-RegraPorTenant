using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Contract;
using RuleKernel.Core.Data;
using RuleKernel.Core.Models;
using RuleKernel.Core.Services;

namespace RuleKernel.Regras.Test;

public sealed class RuleRunnerExecutionTimeTests
{
    private static RuleKernelDbContext CreateDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<RuleKernelDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new RuleKernelDbContext(options);
    }

    [Fact]
    public async Task Deve_executar_pipeline_por_tenant_e_medir_tempo()
    {
        await using var db = CreateDb($"rk-{Guid.NewGuid()}");

        var tenantA = new Tenant { Id = Guid.NewGuid(), Name = "salome", IsActive = true, CreatedAt = DateTime.UtcNow };
        var tenantB = new Tenant { Id = Guid.NewGuid(), Name = "rony", IsActive = true, CreatedAt = DateTime.UtcNow };

        var defVencimentoA = new RuleDefinition
        {
            Id = Guid.NewGuid(),
            Name = "SALOME_DataDeVencimento",
            Description = "Contract: DataDeVencimentoContract",
            ContractType = typeof(DataDeVencimentoContract).FullName!,
            CreatedAt = DateTime.UtcNow,
        };

        var defDescontoA = new RuleDefinition
        {
            Id = Guid.NewGuid(),
            Name = "SALOME_CalculoDesconto",
            Description = "Contract: CalculoDescontoContract",
            ContractType = typeof(CalculoDescontoContract).FullName!,
            CreatedAt = DateTime.UtcNow,
        };

        var defVencimentoB = new RuleDefinition
        {
            Id = Guid.NewGuid(),
            Name = "RONY_DataDeVencimento",
            Description = "Contract: DataDeVencimentoContract",
            ContractType = typeof(DataDeVencimentoContract).FullName!,
            CreatedAt = DateTime.UtcNow,
        };

        var defDescontoB = new RuleDefinition
        {
            Id = Guid.NewGuid(),
            Name = "RONY_CalculoDesconto",
            Description = "Contract: CalculoDescontoContract",
            ContractType = typeof(CalculoDescontoContract).FullName!,
            CreatedAt = DateTime.UtcNow,
        };

        var regraVencimentoA = new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantA.Id,
            RuleDefinitionId = defVencimentoA.Id,
            Priority = 1,
            IsActive = true,
            SourceCode = @$"System.Threading.Thread.Sleep(5);
                            contract.OutResult = contract.InDataDeEmissao.Date.AddDays(7);",
        };

        var regraDescontoA = new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantA.Id,
            RuleDefinitionId = defDescontoA.Id,
            Priority = 1,
            IsActive = true,
            SourceCode = @$"System.Threading.Thread.Sleep(5);
                            contract.OutDesconto = contract.InValorPrincipal >= 1000m 
                                ? contract.InValorPrincipal * 0.05m 
                                : 0m;
                            contract.OutValorTotal = contract.InValorPrincipal - contract.OutDesconto;
                            contract.OutResult = contract.OutValorTotal;",
        };

        var regraVencimentoB = new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantB.Id,
            RuleDefinitionId = defVencimentoB.Id,
            Priority = 1,
            IsActive = true,
            SourceCode = @$"System.Threading.Thread.Sleep(6);
                            contract.OutResult = contract.InDataDeEmissao.Date.AddDays(10);",
        };

        var regraDescontoB = new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantB.Id,
            RuleDefinitionId = defDescontoB.Id,
            Priority = 1,
            IsActive = true,
            SourceCode = @$"System.Threading.Thread.Sleep(6);
                            contract.OutDesconto = 0m;
                            contract.OutValorTotal = contract.InValorPrincipal;
                            contract.OutResult = contract.OutValorTotal;",
        };

        tenantA.RegraDataVencimentoId = regraVencimentoA.Id;
        tenantA.RegraCalculoDescontoId = regraDescontoA.Id;
        tenantB.RegraDataVencimentoId = regraVencimentoB.Id;
        tenantB.RegraCalculoDescontoId = regraDescontoB.Id;

        db.AddRange(
            tenantA,
            tenantB,
            defVencimentoA,
            defDescontoA,
            defVencimentoB,
            defDescontoB,
            regraVencimentoA,
            regraDescontoA,
            regraVencimentoB,
            regraDescontoB);
        await db.SaveChangesAsync();

        var executor = new ConsoleScriptRuleExecutor();
        var runner = new RuleRunner(db, executor);

        var inicio = DateTime.UtcNow;

        var vencA = new DataDeVencimentoContract { InDataDeEmissao = new DateTime(2026, 1, 10) };
        await runner.ExecutarRegra("SALOME_DataDeVencimento", vencA);

        var descA = new CalculoDescontoContract
        {
            InTenantId = tenantA.Id,
            InFaturaId = Guid.NewGuid(),
            InDataDeEmissao = new DateTime(2026, 1, 10),
            InValorPrincipal = 1500m,

        };
        await runner.ExecutarRegra("SALOME_CalculoDesconto", descA);

        var vencB = new DataDeVencimentoContract { InDataDeEmissao = new DateTime(2026, 1, 10) };
        await runner.ExecutarRegra("RONY_DataDeVencimento", vencB);

        var descB = new CalculoDescontoContract
        {
            InTenantId = tenantB.Id,
            InFaturaId = Guid.NewGuid(),
            InDataDeEmissao = new DateTime(2026, 1, 10),
            InValorPrincipal = 1500m,

        };
        await runner.ExecutarRegra("RONY_CalculoDesconto", descB);

        var fim = DateTime.UtcNow;
        var duracao = fim - inicio;

        Assert.Equal(new DateTime(2026, 1, 17), vencA.OutDataVencimento);
        Assert.Equal(new DateTime(2026, 1, 20), vencB.OutDataVencimento);

        Assert.True(duracao.TotalMilliseconds >= 10);
    }
}
