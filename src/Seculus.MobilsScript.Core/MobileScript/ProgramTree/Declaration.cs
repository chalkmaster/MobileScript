using Seculus.MobileScript.Core.MobileScript.Compiler;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree
{
    /// <summary>
    /// Classe abstrata a partir da qual são derivadas as classes que representam as declarações da linguagem.
    /// </summary>
    public abstract class Declaration : Node
    {
        #region Properties

        /// <summary>
        /// Nome do item declarado
        /// </summary>
        public string Name { get; private set; }

        #endregion

        #region Constructors

        protected Declaration(string name, LexSymbol symbol) : base(symbol)
        {
            Name = name;
        }

        #endregion
    }
}
