using RuleKernel.Core.Contract;
using RuleKernel.Core.Services;
using Xunit;

namespace RuleKernel.Regras.Test;

public sealed class ConsoleScriptRuleExecutorSourceCodeTests
{
    public sealed class TestContract : IRuleContract<int>
    {
        public int InValor { get; init; }
        public bool OutErro { get; set; }
        public string? OutMensagem { get; set; }
        public int OutResult { get; set; }
    }


    [Fact]
    public async Task Deve_executar_SourceCode_e_atualizar_contract()
    {
        var executor = new ConsoleScriptRuleExecutor();
        var contract = new TestContract { InValor = 41 };
        var sourceCode = "contract.OutResult = contract.InValor + 1;";

        await executor.ExecuteAsync(sourceCode, contract);

        Assert.Equal(42, contract.OutResult);
    }

    [Fact]
    public async Task Deve_expor_host_no_script()
    {
        var executor = new ConsoleScriptRuleExecutor();
        var contract = new TestContract { InValor = 1 };
        var sourceCode = "host.Imprimir(\"ok\"); contract.OutResult = 10;";

        await executor.ExecuteAsync(sourceCode, contract);

        Assert.Equal(10, contract.OutResult);
    }

    [Fact]
    public async Task Deve_lancar_quando_SourceCode_invalido()
    {
        var executor = new ConsoleScriptRuleExecutor(); 
        var contract = new TestContract();
        var sourceCode = "contract.OutResult = ;";

        await Assert.ThrowsAsync<InvalidOperationException>(() => executor.ExecuteAsync(sourceCode, contract));
    }

    public static TheoryData<TestContract, int> Contratos { get; } = new()
    {
        { new TestContract { InValor = 1 }, 2 },
        { new TestContract { InValor = 41 }, 42 },
    };

    [Theory]
    [MemberData(nameof(Contratos))]
    public void Regra(TestContract contract, int esperado)
    {
        // act
        contract.OutResult = contract.InValor + 1;

        // assert
        Assert.Equal(esperado, contract.OutResult);
    }
}
