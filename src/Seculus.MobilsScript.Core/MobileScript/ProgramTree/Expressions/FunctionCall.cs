using System.Collections.Generic;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;
using Seculus.MobileScript.Core.MobileScript.Symbols;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions
{
    /// <summary>
    /// Descreve uma chamada de função (numa expressão)
    /// </summary>
    public class FunctionCall : Expression
    {
        #region Properties

        /// <summary>
        /// Nome da função
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Lista de parâmetros
        /// </summary>
        public IList<Expression> Parameters { get; private set; }

        /// <summary>
        /// Símbolo na tabela que descreve esta função
        /// </summary>
        public FunctionSymbol FunctionSymbol { get; internal set; }

        /// <summary>
        /// Tipo de retorno da função.
        /// </summary>
        public override TypeDeclaration ReturnType
        {
            get
            {
                if (FunctionSymbol != null)
                {
                    return FunctionSymbol.Type;
                }
                return null;
            }
        }

        #endregion

        #region Constructors

        public FunctionCall(string name, LexSymbol symbol) : base(symbol)
        {
            Name = name;
            Parameters = new List<Expression>();
        }

        #endregion

        #region Methods

        public override object Accept(INodeVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var codeGen = new ToStringVisitor();
            return codeGen.Visit(this).ToString();
        }

        #endregion
    }
}
