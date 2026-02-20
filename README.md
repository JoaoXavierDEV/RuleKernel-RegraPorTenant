
# RuleKernel

## Execução do teste (API + Blazor WebAssembly)

Há uma API e um Blazor WebAssembly que faz `GET` na API para buscar as regras. Para o teste, ambos devem ser executados juntos.

Pré-requisito: `.NET 10` instalado.

1. Iniciar a API:
   - `dotnet run --project src/RuleKernel.Api`
2. Iniciar o Blazor WebAssembly:
   - `dotnet run --project src/BlazorApp1`

### Executar no Visual Studio

1. Abrir a solução no Visual Studio.
2. Em **Configuração de Inicialização**, selecionar **Vários projetos de inicialização**.
3. Definir `RuleKernel.Api` e `BlazorApp1` como **Iniciar**.
4. Pressionar **F5**.

## Execução de regras C# (ambiente do script)

As regras são executadas como **C# Script** (Roslyn Scripting) em runtime, pela classe `ConsoleScriptRuleExecutor`.

### Como a regra é executada

- O código da regra (campo `SourceCode`) é compilado e executado via `Microsoft.CodeAnalysis.CSharp.Scripting`:
  - `CSharpScript.Create(source, options: ..., globalsType: ...)`
  - `script.Compile(...)`
  - `script.RunAsync(globals, ...)`

Na prática, o script roda como **top-level statements**, dentro de um tipo gerado dinamicamente pelo Roslyn.

### Variáveis globais disponíveis na regra

O script recebe um objeto de *globals* do tipo `ConsoleScriptRuleExecutor.ScriptGlobals<TContract>`, instanciado com:

- `contract`: o contrato/DTO passado no `ExecuteAsync(source, contract, ...)` (tipado como `TContract?`).
- `host`: uma instância de `ConsoleScriptRuleExecutor.RegraBaseHost : RegraBase` para utilitários (por exemplo, `host.Imprimir(...)`).

Assim, uma regra pode acessar diretamente `contract` e `host`, por exemplo:

- `contract.OutResult = contract.InDataDeCredito.AddDays(7);`
- `host.Imprimir(contract.InDataDeCredito.ToString());`

### Imports e referências

O executor cria as opções do script em `CreateScriptOptions()`, definindo:

- **References**: todas as assemblies já carregadas no `AppDomain.CurrentDomain` (não dinâmicas e com `Location`).
- **Imports** automáticos:
  - `System`
  - `System.Threading`
  - `System.Threading.Tasks`
  - `RuleKernel.Services`
  - `RuleKernel.Contract`

### Exemplo Prático
```csharp
public async Task<DateTime> CalcularDataDeVencimentoAsync(CancellationToken cancellationToken = default)
{
    var contrato = new DataDeVencimentoContract
    {
        InDataDeEmissao = DateTime.Now
    };        
        
    // regra pode ser associada ao tenant
    await _ruleRunner.ExecutarRegra("SALOME_DataDeVencimento", contrato, cancellationToken);

    return contrato.OutResult;
}
```

```csharp
public async Task<DateTime> CalcularDataDeVencimentoAsync(Guid tenantId)
{
    var usuario = await _db.Tenants
        .Include(t => t.RegraDataVencimento)!
            .ThenInclude(r => r!.RuleDefinition)
        .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);

    if (usuario is null)
        throw new InvalidOperationException($"Tenant não encontrado/ativo: '{tenantId}'.");

    if (usuario.RegraDataVencimento?.RuleDefinition?.Name is null)
        throw new InvalidOperationException("Tenant sem RegraDataVencimento associada.");

    var contrato = new DataDeVencimentoContract
    {
        InDataDeEmissao = DateTime.Now
    };         

    await _ruleRunner.ExecutarRegra(usuario.RegraDataVencimento!.RuleDefinition!.Name, contrato);

    return contrato.OutResult;
}
```