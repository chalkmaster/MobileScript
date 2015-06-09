using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;
using Seculus.MobileScript.Core.MobileScript.Symbols;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements
{
    /// <summary>
    /// Descreve o comando de retorno (return).
    /// </summary>
    public class ReturnStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Expressão que define o valor de retorno
        /// </summary>
        public Expression ValueToReturn { get; private set; }

        /// <summary>
        /// Símbolo na tabela que representa a função que contém esse comando de return.
        /// </summary>
        public FunctionSymbol FunctionSymbol { get; internal set; }

        public override TypeDeclaration ReturnType
        {
            get { return ValueToReturn.ReturnType; }
        }

        #endregion

        #region Constructors

        public ReturnStatement(Expression valueToReturn, LexSymbol symbol) : base(symbol)
        {
            ValueToReturn = valueToReturn;
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
