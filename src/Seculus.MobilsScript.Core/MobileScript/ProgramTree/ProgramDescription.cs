using System.Collections.Generic;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;
using Seculus.MobileScript.Core.MobileScript.Symbols;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree
{
    /// <summary>
    /// Descreve um programa.
    /// </summary>
    public class ProgramDescription : Node
    {
        #region Properties

        /// <summary>
        /// Lista com as declarações feitas no programa.
        /// </summary>
        public IList<Declaration> Declarations { get; private set; }

        /// <summary>
        /// Corpo do programa (função main).
        /// </summary>
        public CompoundStatement Body { get; internal set; }

        /// <summary>
        /// Escopo associado ao programa.
        /// </summary>
        public Scope Scope { get; internal set; }

        #endregion

        #region Constructors

        public ProgramDescription(LexSymbol symbol) : base(symbol)
        {
            Declarations = new List<Declaration>();
            Body = new CompoundStatement(symbol);
        }

        #endregion

        #region Public Methods

        public override object Accept(INodeVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var codeGen = new ToStringVisitor();
            return Accept(codeGen).ToString();
        }

        #endregion
    }
}
