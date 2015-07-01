using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;
using Seculus.MobileScript.Core.MobileScript.Symbols;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations
{
    /// <summary>
    /// Descreve uma declaração de variável ou parâmetro.
    /// </summary>
    public class VariableDeclaration : Declaration
    {
        #region Constructors

        public VariableDeclaration(string name, TypeDeclaration type, Expression initialValue, LexSymbol symbol) 
            : this(name, type, initialValue, false, symbol)
        {
        }

        public VariableDeclaration(string name, TypeDeclaration type, Expression initialValue, bool isRef, LexSymbol symbol)
            : base(name, symbol)
        {
            Type = type;
            InitialValue = initialValue;
            IsRef = isRef;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tipo da variável declarada.
        /// </summary>
        public TypeDeclaration Type { get; private set; }

        /// <summary>
        /// Valor inicial
        /// </summary>
        public Expression InitialValue { get; private set; }

        /// <summary>
        /// Se true, indica passagem por referência.
        /// </summary>
        public bool IsRef { get; private set; }

        /// <summary>
        /// Símbolo na tabela que descreve esta declaração.
        /// </summary>
        public VariableSymbol VariableSymbol { get; internal set; }

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
