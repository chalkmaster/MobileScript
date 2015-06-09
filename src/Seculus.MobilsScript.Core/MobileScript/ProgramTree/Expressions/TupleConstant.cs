using System;
using System.Collections.Generic;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions
{
    /// <summary>
    /// Descreve uma tupla (sequência de constantes entre '{' e '}', separadas por vírgula).
    /// </summary>
    public class TupleConstant : Expression
    {
        #region Fields

        private TypeDeclaration _returnType;

        #endregion

        #region Properties

        /// <summary>
        /// Elementos que integram a tupla.
        /// </summary>
        public IList<Expression> Elements { get; private set; }

        /// <summary>
        /// Tipo da tupla.
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

        public TupleConstant(IList<Expression> elements, LexSymbol lexSymbol)
            : base(lexSymbol)
        {
            Elements = elements;
        }

        #endregion

        #region Private/Internal Methods

        internal void DefineReturnType(TypeDeclaration type)
        {
            _returnType = type;
        }

        private void DefineReturnType()
        {
            if (Elements == null) throw new NullReferenceException("Unexpected null Elements");
            if (Elements.Count > 0)
            {
                TypeDeclaration elementType = Elements[0].ReturnType;
                _returnType = new VectorTypeDeclaration(Elements.Count, elementType, LexSymbol);
            }
            else
            {
                _returnType = TypeDeclaration.Wrong;
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
