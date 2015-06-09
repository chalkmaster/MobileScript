using Seculus.MobileScript.Core.Extensions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree;

namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Representa um símbolo constante
    /// </summary>
    public class ConstantSymbol : Symbol
    {
        #region Constructors

        public ConstantSymbol(string name, int level, Node value) : base(name, level, SymbolKind.Constant)
        {
            Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Valor da constante (string ou tupla)
        /// </summary>
        public Node Value { get; private set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return "[const name:{0} level:{1} value:{2}]".FormatWith(Name, Level, Value);
        }

        #endregion
    }
}
