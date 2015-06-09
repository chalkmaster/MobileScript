using Seculus.MobileScript.Core.MobileScript.Compiler;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions
{
    /// <summary>
    /// Classe abstrata base para as classes que representam os comandos da linguagem.
    /// </summary>
    public abstract class Statement : Expression
    {
        protected Statement(LexSymbol symbol) : base(symbol)
        {
        }
    }
}
