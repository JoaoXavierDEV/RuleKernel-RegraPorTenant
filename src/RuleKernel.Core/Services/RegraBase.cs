using System.Diagnostics;

namespace RuleKernel.Core.Services;

public abstract class RegraBase
{
    public void Imprimir(string mensagem)
    {
        Console.WriteLine(string.Format("Regra Base: {0}",mensagem));
    }
}
