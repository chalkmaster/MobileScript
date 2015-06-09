using Seculus.MobileScript.Core.MobileScript.Compiler;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions
{
    /// <summary>
    /// Representa uma constante usada como operando numa expressão.
    /// Uma constante pode ser:
    ///    - uma constante inteira (iniciando por um dígito decimal)
    ///    - uma constante boolean ('true' ou 'false', que pode estar na tabela de símbolos)
    ///    - uma constante do tipo string (entre aspas duplas)
    ///    - uma tupla (lista de constantes entre '{' e '}', separadas por vírgula
    /// </summary>
    public class Constant : Expression
    {
        #region Fields

        private readonly TypeDeclaration _returnType;

        #endregion

        #region Properties

        /// <summary>
        /// Valor da constant representado como string
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Tipo da constante.
        /// </summary>
        public override TypeDeclaration ReturnType { get { return _returnType; } }

        #endregion

        #region Constructors

        public Constant(string value, TypeDeclaration type, LexSymbol symbol)
            : base(symbol)
        {
            Value = value;
            _returnType = type;
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
