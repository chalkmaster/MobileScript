using Seculus.MobileScript.Core.Extensions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;

namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Representa um símbolo de parâmetro de função.
    /// </summary>
    public class ParameterSymbol : VariableSymbol
    {
        #region Properties

        /// <summary>
        /// True indica passagem de parâmetro por referência.
        /// </summary>
        public bool ByRef { get; private set; }

        #endregion

        #region Constructors

        public ParameterSymbol(string name, TypeDeclaration type, bool byRef)
            : base(name, (int)SymbolLevel.Local, type, SymbolKind.Parameter)
        {
            ByRef = byRef;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return "[param name:{0} level:{1} type:{2} ref:{3}]".FormatWith(Name, Level, Type, ByRef);
        }

        #endregion
    }
}
