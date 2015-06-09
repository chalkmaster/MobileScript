using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;
using Seculus.MobileScript.Core.MobileScript.Symbols;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions
{
    /// <summary>
    /// Descreve uma variável usada numa expressão
    /// </summary>
    public class Variable : Expression
    {
        #region Fields

        private TypeDeclaration _returnType;

        #endregion

        #region Properties

        /// <summary>
        /// Nome da variável.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Símbolo que descreve a variável na tabela
        /// </summary>
        public VariableSymbol VariableSymbol { get; internal set; }

        /// <summary>
        /// Tipo da variável
        /// </summary>
        public override TypeDeclaration ReturnType
        {
            get { return _returnType; }
        }

        #endregion

        #region Constructors

        public Variable(string name, LexSymbol symbol)
            : base(symbol)
        {
            Name = name;
            _returnType = null;
        }

        #endregion

        #region Methods

        public void DefineReturnType(TypeDeclaration type)
        {
            _returnType = type;
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
