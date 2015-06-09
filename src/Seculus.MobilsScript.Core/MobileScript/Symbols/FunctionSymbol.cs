using System.Collections.Generic;
using Seculus.MobileScript.Core.Extensions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;

namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Representa um símbolo de função.
    /// </summary>
    public class FunctionSymbol : VariableSymbol
    {
        #region Properties

        /// <summary>
        /// Lista com a definição dos parâmetros formais
        /// </summary>
        public IList<ParameterSymbol> Parameters { get; internal set; }

        /// <summary>
        /// Comando que descreve o corpo da função.
        /// </summary>
        public Expression Body { get; internal set; }

        /// <summary>
        /// Escopo contendo os símbolos locais à função.
        /// </summary>
        public Scope Scope { get; internal set; }

        /// <summary>
        /// Endereço inicial da função
        /// </summary>
        public int StartAddress { get; internal set; }

        #endregion

        #region Constructors

        public FunctionSymbol(string name, TypeDeclaration type) : base(name, (int)SymbolLevel.PreDefined, type, SymbolKind.Function)
        {
            Parameters = new List<ParameterSymbol>();
            Body = null;
            Scope = null;
            StartAddress = -99999;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retorna o endereço relativo (a partir do endereço base da função) do valor de retorno da função.
        /// </summary>
        /// <returns>Endereço relativo da variável que vai receber o retorno da função.</returns>
        public int GetRelativeReturnValueAddress()
        {
            // Endereço relativo da variável que vai receber o valor de retorno.
            // O retorno da função entra na pilha antes dos parâmetros ( -Scope.ParametersSize ) 
            // e também antes dois dois ( -2 ) valores que são empilhados para controle interno da VM (_pc e _base).
            return -Scope.ParametersSize - 2;
        }

        public override string ToString()
        {
            return "[function name:{0} level:{1} type:{2}]".FormatWith(Name, Level, Type);
        }

        #endregion
    }
}
