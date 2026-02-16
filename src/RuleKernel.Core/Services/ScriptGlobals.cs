namespace RuleKernel.Core.Services;

public sealed class ScriptGlobals<TContract>
{
    public ScriptGlobals(ConsoleScriptRuleExecutor.RegraBaseHost host, TContract contract)
    {
        Host = host;
        Contract = contract;
    }

    // Nomes intencionais para uso direto no script.
    public ConsoleScriptRuleExecutor.RegraBaseHost Host { get; }
    public TContract Contract { get; }

    public ConsoleScriptRuleExecutor.RegraBaseHost host => Host;
    public TContract contract => Contract;
}
