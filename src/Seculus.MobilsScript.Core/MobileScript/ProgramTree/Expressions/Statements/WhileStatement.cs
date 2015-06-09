using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements
{
    /// <summary>
    /// Descreve um comando de repetição (while)
    /// </summary>
    public class WhileStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Condição da repetição
        /// </summary>
        public Expression Condition { get; private set; }

        /// <summary>
        /// Comando de repetição.
        /// </summary>
        public Statement Body { get; private set; }

        public override TypeDeclaration ReturnType
        {
            get { return TypeDeclaration.Void; }
        }

        #endregion

        #region Constructors

        public WhileStatement(Expression condition, Statement body, LexSymbol symbol) : base(symbol)
        {
            Condition = condition;
            Body = body;
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
