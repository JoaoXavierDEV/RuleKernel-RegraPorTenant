# RuleKernel

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
