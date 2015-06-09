using System;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions
{
    /// <summary>
    /// Descreve uma operação unária numa expressão ('-' ou 'NOT')
    /// </summary>
    public class UnaryOperation : Expression
    {
        #region Fields

        private TypeDeclaration _returnType;

        #endregion

        #region Properties

        /// <summary>
        /// Tipo da operação
        /// </summary>
        public RplOperationType OperationType { get; private set; }

        /// <summary>
        /// Operando
        /// </summary>
        public Expression Operand { get; private set; }

        public override TypeDeclaration ReturnType
        {
            get
            {
                if (_returnType == null)
                {
                    DefineReturnType();
                }
                return _returnType;
            }
        }

        #endregion

        #region Constructors

        public UnaryOperation(RplOperationType operationType, Expression operand, LexSymbol symbol)
            : base(symbol)
        {
            OperationType = operationType;
            Operand = operand;
        }

        #endregion

        #region Private Methods

        private void DefineReturnType()
        {
            if (Operand == null) throw new ApplicationException("Unexpected null Operand.");
            if (Operand.ReturnType == null) throw new ApplicationException("Unexpected null Operand.ReturnType.");
            _returnType = Operand.ReturnType;
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
            return codeGen.Visit(this).ToString();
        }

        #endregion
    }
}
