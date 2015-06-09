using Seculus.MobileScript.Core.Helpers;
using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations
{
    public class TypeDeclaration : Declaration
    {
        #region Constructors

        // To be used by primitive types only.
        private TypeDeclaration(string name) : this(name, null) { }

        public TypeDeclaration(string name, LexSymbol symbol)
            : base(name, symbol)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Devolve o espaço na pilha ocupado por um valor deste tipo.
        /// Valores dos tipos primitivos ocupam uma posição (para a referência ao objeto, na MobileVM)
        /// </summary>
        public virtual int TotalSize
        {
            get
            {
                return 1;
            }
        }

        #endregion

        #region Static Instances - Primitive Types

        public static readonly TypeDeclaration Int = new TypeDeclaration(Keywords.Types.Int);
        public static readonly TypeDeclaration Bool = new TypeDeclaration(Keywords.Types.Boolean);
        public static readonly TypeDeclaration Void = new TypeDeclaration(Keywords.Types.Void);
        public static readonly TypeDeclaration String = new TypeDeclaration(Keywords.Types.String);
        public static readonly TypeDeclaration Float = new TypeDeclaration(Keywords.Types.Float);
        public static readonly TypeDeclaration Tuple = new TypeDeclaration(RplConstants.TupleTypeName);

        public static readonly TypeDeclaration Wrong = new TypeDeclaration(RplConstants.WrongTypeName);

        #endregion

        #region Public Methods

        /// <summary>
        /// Retorna o tipo.
        /// </summary>
        /// <returns>Tipo</returns>
        public virtual TypeDeclaration GetBaseType()
        {
            return this;
        }

        /// <summary>
        /// Verifica se um tipo é numérico (int ou float).
        /// </summary>
        /// <returns></returns>
        public bool IsNumeric()
        {
            return Int.Equals(this) || Float.Equals(this);
        }

        /// <summary>
        /// Verifica se esse tipo é um tipo primitivo.
        /// </summary>
        public bool IsPrimitive()
        {
            return
                this == TypeDeclaration.Int ||
                this == TypeDeclaration.Bool ||
                this == TypeDeclaration.String ||
                this == TypeDeclaration.Float;
        }

        public bool IsVector()
        {
            return this is VectorTypeDeclaration;
        }

        /// <summary>
        /// Verifica se esse tipo é comparável com o outro tipo informado.
        /// </summary>
        /// <param name="type">Tipo que se deseja comparar com esse.</param>
        /// <returns>True se os dois tipos forem comparáveis. Caso contrário, false.</returns>
        public bool IsComparableTo(TypeDeclaration type)
        {
            Check.Argument.IsNotNull(type, "type");
            if (this is VectorTypeDeclaration || type is VectorTypeDeclaration) return false;
            if (type.Equals(this)) return true;
            if (IsNumeric() && type.IsNumeric()) return true;
            return false;
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
