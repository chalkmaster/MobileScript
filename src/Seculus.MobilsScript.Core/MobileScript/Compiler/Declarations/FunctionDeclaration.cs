using System.Collections.Generic;
using System.Collections.ObjectModel;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;
using Seculus.MobileScript.Core.MobileScript.Symbols;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations
{
    public class FunctionDeclaration : Declaration
    {
        #region Properties

        /// <summary>
        /// Tipo de retorno da função
        /// </summary>
        public TypeDeclaration ReturnType { get; private set; }

        /// <summary>
        /// Parâmetros da função
        /// </summary>
        public ReadOnlyCollection<VariableDeclaration> Parameters { get; private set; }

        /// <summary>
        /// Corpo da função
        /// </summary>
        public CompoundStatement Body { get; private set; }

        public FunctionSymbol FunctionSymbol { get; internal set; }

        #endregion

        #region Constructors

        public FunctionDeclaration(string name, TypeDeclaration returnType, IList<VariableDeclaration> parameters, CompoundStatement body, LexSymbol symbol)
            : base(name, symbol)
        {
            ReturnType = returnType;
            Parameters = new ReadOnlyCollection<VariableDeclaration>(parameters);
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
