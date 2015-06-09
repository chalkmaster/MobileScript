using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements
{
    /// <summary>
    /// Descreve um comando condicional ('if')
    /// </summary>
    public class IfStatement : Statement
    {
        #region Properties

        public Expression Condition { get; private set; }
        public Statement ThenPart { get; private set; }
        public Statement ElsePart { get; private set; }

        public override TypeDeclaration ReturnType
        {
            get { return TypeDeclaration.Void; }
        }

        #endregion

        #region Constructors

        public IfStatement(Expression condition, Statement thenPart, Statement elsePart, LexSymbol symbol)
            : base(symbol)
        {
            Condition = condition;
            ThenPart = thenPart;
            ElsePart = elsePart;
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
