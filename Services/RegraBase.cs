using System.Diagnostics;

namespace RuleKernel.Services
{
    public abstract class RegraBase
    {
        public void Imprimir(string mensagem)
        {
            Debug.WriteLine(mensagem);
        }
    }
}
