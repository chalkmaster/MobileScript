using System;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions
{
    /// <summary>
    /// Descreve uma operação diádica (2 operandos)
    /// </summary>
    public class DyadicOperation : Expression
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
        /// Primeiro operando
        /// </summary>
        public Expression Operand1 { get; private set; }

        /// <summary>
        /// Segundo operando
        /// </summary>
        public Expression Operand2 { get; private set; }

        /// <summary>
        /// Tipo de retorno da operação diádica.
        /// </summary>
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

        public DyadicOperation(RplOperationType operationType, Expression operand1, Expression operand2, LexSymbol symbol)
            : base(symbol)
        {
            OperationType = operationType;
            Operand1 = operand1;
            Operand2 = operand2;
        }

        #endregion

        #region Private Methods

        private void DefineReturnType()
        {
            if (Operand1 == null) throw new NullReferenceException("Unexpected null Operand1");
            if (Operand2 == null) throw new NullReferenceException("Unexpected null Operand2");
            if (Operand1.ReturnType == null) throw new NullReferenceException("Unexpected null Operand1.Type");
            if (Operand2.ReturnType == null) throw new NullReferenceException("Unexpected null Operand2.Type");

            if (OperationType == RplOperationType.Add && 
                (Operand1.ReturnType == TypeDeclaration.String && Operand2.ReturnType == TypeDeclaration.String))
            {
                _returnType = TypeDeclaration.String;
            } 
            else if (OperationType == RplOperationType.Add ||
                     OperationType == RplOperationType.Subtract ||
                     OperationType == RplOperationType.Divide ||
                     OperationType == RplOperationType.Multiply ||
                     OperationType == RplOperationType.Module)
            {
                if (!Operand1.ReturnType.IsNumeric() || !Operand2.ReturnType.IsNumeric())
                {
                    _returnType = TypeDeclaration.Wrong;
                }
                else if (Operand1.ReturnType == TypeDeclaration.Float || Operand2.ReturnType == TypeDeclaration.Float)
                {
                    _returnType = TypeDeclaration.Float;
                }
                else
                {
                    _returnType = TypeDeclaration.Int;
                }
            }
            else if (OperationType == RplOperationType.Equal ||
                     OperationType == RplOperationType.NotEqual ||
                     OperationType == RplOperationType.GreaterOrEqual ||
                     OperationType == RplOperationType.GreaterThan ||
                     OperationType == RplOperationType.LessOrEqual ||
                     OperationType == RplOperationType.LessThan)
            {
                if (Operand1.ReturnType.IsComparableTo(Operand2.ReturnType))
                {
                    _returnType = TypeDeclaration.Bool;
                }
                else
                {
                    _returnType = TypeDeclaration.Wrong;
                }
            }
            else if (OperationType == RplOperationType.And ||
                     OperationType == RplOperationType.Or)
            {
                if (Operand1.ReturnType == TypeDeclaration.Bool && Operand2.ReturnType == TypeDeclaration.Bool)
                {
                    _returnType = TypeDeclaration.Bool;
                }
                else
                {
                    _returnType = TypeDeclaration.Wrong;
                }
            }
            else
            {
                throw new ApplicationException("Unexpected diadic operation type.");
            }
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
