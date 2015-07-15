using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations
{
    /// <summary>
    /// Descreve um tipo vetor (anônimo)
    /// </summary>
    public class VectorTypeDeclaration : TypeDeclaration
    {
        #region Constructors

        public VectorTypeDeclaration(int? size, TypeDeclaration elementType, LexSymbol symbol)
            : base("~VectorType", symbol) // tipo anônimo
        {
            Size = size;
            ElementType = elementType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tamanho do vetor.
        /// NULL representa um vetor com tamanho indefinido.
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// Tipo dos elementos do vetor
        /// </summary>
        public TypeDeclaration ElementType { get; private set; }

        public override int TotalSize
        {
            get
            {
                return (Size ?? -1) * ElementType.TotalSize;
            }
        }

        #endregion

        #region Methods

        public override TypeDeclaration GetBaseType()
        {
            return ElementType.GetBaseType();
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
