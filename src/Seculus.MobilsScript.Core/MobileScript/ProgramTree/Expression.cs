using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree
{
    /// <summary>
    /// Classe abstrata base para as classes que descrevem as expressões da linguagem.
    /// </summary>
    public abstract class Expression : Node
    {
        #region Constructors

        protected Expression(LexSymbol symbol)
            : base(symbol)
        {
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Tipo de retorno dessa expressão.
        /// </summary>
        public abstract TypeDeclaration ReturnType { get; }

        #endregion
    }
}
