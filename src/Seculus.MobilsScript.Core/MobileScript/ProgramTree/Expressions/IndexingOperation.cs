using System;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions
{
    /// <summary>
    /// Descreve uma indexação.
    /// </summary>
    public class IndexingOperation : Expression
    {
        #region Fields

        /// <summary>
        /// Tipo de retorno da função.
        /// </summary>
        private TypeDeclaration _returnType;

        #endregion

        #region Properties

        /// <summary>
        /// Primeiro operando
        /// </summary>
        public Expression Operand1 { get; private set; }

        /// <summary>
        /// Segundo operando
        /// </summary>
        public Expression Operand2 { get; private set; }

        /// <summary>
        /// Tipo de retorno da indexação.
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

        public IndexingOperation(Expression operand1, Expression operand2, LexSymbol symbol) : base(symbol)
        {
            Operand1 = operand1;
            Operand2 = operand2;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retorna o tipo dos elementos desse vetor.
        /// </summary>
        /// <returns>Tipo dos elementos do vetor.</returns>
        public TypeDeclaration GetElementType()
        {
            Expression operation = Operand1 as Variable;
            if (operation == null) operation = Operand1 as IndexingOperation;
            if (operation == null) throw new NotImplementedException("First operand of an IndexingOperation should be Variable or another IndexingOperation (for multi-dimensional arrays).");

            var vectorType = operation.ReturnType as VectorTypeDeclaration;
            if (vectorType == null) throw new NotImplementedException("First operand of an IndexingOperation should be of type VectorType.");
            return vectorType.ElementType;
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

        #region Private Methods

        private void DefineReturnType()
        {
            if (Operand1 == null) throw new NullReferenceException("Unexpected null Operand1");

            TypeDeclaration elementType = GetElementType();
            _returnType = elementType;
        }

        #endregion
    }
}
