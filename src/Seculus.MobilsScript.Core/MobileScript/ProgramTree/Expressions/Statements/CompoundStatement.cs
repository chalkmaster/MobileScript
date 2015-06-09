using System.Collections.Generic;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements
{
    /// <summary>
    /// Descreve um comando composto ou um 'corpo de função' (ou do 'main')
    /// </summary>
    public class CompoundStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Declarações feitas no programa
        /// </summary>
        public IList<Declaration> Declarations { get; private set; }

        /// <summary>
        /// Código de inicialização de variáveis (se for o caso)
        /// </summary>
        public IList<Assignment> Initializations { get; private set; }

        /// <summary>
        /// Comandos
        /// </summary>
        public IList<Statement> Statements { get; private set; }

        public override TypeDeclaration ReturnType
        {
            get { return TypeDeclaration.Void; }
        }

        #endregion

        #region Constructors

        public CompoundStatement(LexSymbol symbol) : this(new List<Declaration>(), new List<Assignment>(),  new List<Statement>(), symbol) { }

        private CompoundStatement(IList<Declaration> declarations, IList<Assignment> initializationCode, IList<Statement> statements, LexSymbol symbol) : base(symbol)
        {
            Declarations = declarations;
            Initializations = initializationCode;
            Statements = statements;
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
