using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Contract;
using RuleKernel.Core.Data;
using RuleKernel.Core.Models;
using RuleKernel.Core.Services;
using Xunit;

namespace RuleKernel.Regras.Test.Regras.Rony;

public sealed class Rony_CalculoDesconto_Tests
{
    [Fact]
    public async Task Deve_calcular_desconto_para_rony()
    {
        var options = new DbContextOptionsBuilder<RuleKernelDbContext>()
            .UseInMemoryDatabase($"rk-{Guid.NewGuid()}")
            .Options;

        await using var db = new RuleKernelDbContext(options);

        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "rony", IsActive = true, CreatedAt = DateTime.UtcNow };

        var def = new RuleDefinition
        {
            Id = Guid.NewGuid(),
            Name = "RONY_CalculoDesconto",
            ContractType = typeof(CalculoDescontoContract).FullName!,
            CreatedAt = DateTime.UtcNow,
        };

        var regra = new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            RuleDefinitionId = def.Id,
            Priority = 1,
            IsActive = true,
            SourceCode = "contract.OutDesconto = 0m; contract.OutValorTotal = contract.InValorPrincipal; contract.OutResult = contract.OutValorTotal;",
        };

        db.AddRange(tenant, def, regra);
        await db.SaveChangesAsync();

        var runner = new RuleRunner(db, new ConsoleScriptRuleExecutor());

        var contrato = new CalculoDescontoContract
        {
            InTenantId = tenant.Id,
            InFaturaId = Guid.NewGuid(),
            InDataDeEmissao = new DateTime(2026, 1, 10),
            InValorPrincipal = 1500m,
            InPercentualTaxaAdministracao = 0m,
            InPercentualDesconto = 0m,
        };

        await runner.ExecutarRegra("RONY_CalculoDesconto", contrato);

        Assert.Equal(0m, contrato.OutDesconto);
        Assert.Equal(1500m, contrato.OutValorTotal);
        Assert.Equal(1500m, contrato.OutResult);
    }
}
