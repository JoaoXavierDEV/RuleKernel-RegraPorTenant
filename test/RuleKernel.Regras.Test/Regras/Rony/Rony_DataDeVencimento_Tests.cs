using Microsoft.EntityFrameworkCore;
using RuleKernel.Core.Contract;
using RuleKernel.Core.Data;
using RuleKernel.Core.Models;
using RuleKernel.Core.Services;
using Xunit;

namespace RuleKernel.Regras.Test.Regras.Rony;

public sealed class Rony_DataDeVencimento_Tests
{
    [Fact]
    public async Task Deve_calcular_data_de_vencimento_para_rony()
    {
        var options = new DbContextOptionsBuilder<RuleKernelDbContext>()
            .UseInMemoryDatabase($"rk-{Guid.NewGuid()}")
            .Options;

        await using var db = new RuleKernelDbContext(options);

        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "rony", IsActive = true, CreatedAt = DateTime.UtcNow };

        var def = new RuleDefinition
        {
            Id = Guid.NewGuid(),
            Name = "RONY_DataDeVencimento",
            ContractType = typeof(DataDeVencimentoContract).FullName!,
            CreatedAt = DateTime.UtcNow,
        };

        var regra = new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            RuleDefinitionId = def.Id,
            Priority = 1,
            IsActive = true,
            SourceCode = "contract.OutResult = contract.InDataDeEmissao.Date.AddDays(10);",
        };

        db.AddRange(tenant, def, regra);
        await db.SaveChangesAsync();

        var runner = new RuleRunner(db, new ConsoleScriptRuleExecutor());

        var contrato = new DataDeVencimentoContract { InDataDeEmissao = new DateTime(2026, 1, 10) };
        await runner.ExecutarRegra("RONY_DataDeVencimento", contrato);

        Assert.Equal(new DateTime(2026, 1, 20), contrato.OutDataVencimento);
    }
}
