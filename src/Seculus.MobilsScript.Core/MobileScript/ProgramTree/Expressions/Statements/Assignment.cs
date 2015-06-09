using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements
{
    /// <summary>
    /// Descreve um comando de atribuição.
    /// </summary>
    public class Assignment : Statement
    {
        #region Properties

        /// <summary>
        /// Descreve a variável.
        /// </summary>
        public Expression Variable { get; private set; }

        /// <summary>
        /// Descreve a expressão.
        /// </summary>
        public Expression Value { get; private set; }

        /// <summary>
        /// Descreve o tipo de retorno dessa operação.
        /// </summary>
        public override TypeDeclaration ReturnType
        {
            get { return TypeDeclaration.Void; }
        }

        #endregion

        #region Constructors

        public Assignment(Expression variable, Expression value, LexSymbol symbol) : base(symbol)
        {
            Variable = variable;
            Value = value;
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
