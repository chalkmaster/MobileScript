using Seculus.MobileScript.Core.Extensions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;

namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Representa um símbolo de variável.
    /// </summary>
    public class VariableSymbol : Symbol
    {
        #region Constructors

        public VariableSymbol(string name, int level, TypeDeclaration type)
            : this(name, level, type, SymbolKind.Variable)
        {
        }

        protected VariableSymbol(string name, int level, TypeDeclaration type, SymbolKind kind)
            : base(name, level, kind)
        {
            Type = type;
            Address = -99999; // um endereço inválido.
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tipo da variável descrita por este símbolo
        /// </summary>
        public TypeDeclaration Type { get; internal set; }

        /// <summary>
        /// Endereço da variável na pilha (relativo ao reg. base)
        /// </summary>
        public int Address { get; internal set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return "[var name:{0} level:{1} type:{2}]".FormatWith(Name, Level, Type);
        }

        #endregion
    }
}
