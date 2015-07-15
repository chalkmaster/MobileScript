using System.Collections.Generic;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations
{
    /// <summary>
    /// Descreve uma declaração de lista de variáveis (ex: int a[10], b=1, c;)
    /// </summary>
    public class VariableDeclarationList : Declaration
    {
        #region Properties

        /// <summary>
        /// Lista com as variáveis declaradas.
        /// </summary>
        public IList<VariableDeclaration> VariablesDeclarations { get; private set; }

        /// <summary>
        /// Tipo das variáveis declaradas.
        /// </summary>
        public TypeDeclaration Type { get; private set; }

        #endregion

        #region Constructors

        public VariableDeclarationList(TypeDeclaration type, LexSymbol symbol)
            : this(new List<VariableDeclaration>(), type, symbol)
        { }

        private VariableDeclarationList(IList<VariableDeclaration> variables, TypeDeclaration type, LexSymbol symbol)
            : base("?VarDeclList", symbol)
        {
            VariablesDeclarations = variables;
            Type = type;
        }

        #endregion

        #region Methods

        public void Add(VariableDeclaration variableDeclaration)
        {
            VariablesDeclarations.Add(variableDeclaration);
        }

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
