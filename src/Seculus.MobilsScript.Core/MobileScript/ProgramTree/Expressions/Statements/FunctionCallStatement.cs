using System.Collections.Generic;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;
using Seculus.MobileScript.Core.MobileScript.Symbols;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements
{
    /// <summary>
    /// Descreve uma chamada de função (como comando).
    /// </summary>
    public class FunctionCallStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Nome da função
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Lista da parâmetros
        /// </summary>
        public IList<Expression> Parameters { get; private set; }

        /// <summary>
        /// Símbolo na tabela que representa a função chamada
        /// </summary>
        public FunctionSymbol FunctionSymbol { get; internal set; }

        public override TypeDeclaration ReturnType
        {
            get { return TypeDeclaration.Void; }
        }

        #endregion

        #region Constructors

        public FunctionCallStatement(string name, LexSymbol symbol)
            : base(symbol)
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
