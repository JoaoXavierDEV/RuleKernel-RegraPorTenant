# Copilot Instructions

## Diretrizes de projeto
- O usuário sempre executa regras via `IRuleRunner` e não chama `ConsoleScriptRuleExecutor.ExecuteAsync` diretamente.

- Em `RuleKernel.Core.Contract`, todo contract deve ter variáveis/propriedades iniciando com `In` ou `Out` para indicar entrada/saída de dados, seguindo o padrão de `Contracts.cs`.