using Seculus.MobileScript.Core.Extensions;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Messagem de erro de compilação
    /// </summary>
    public class CompilationError
    {
        #region Constructors

        public CompilationError(string errorMessage, CompilationErrorType errorType, LexSymbol symbol)
        {
            Message = errorMessage;
            Type = errorType;
            Symbol = symbol;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Símbolo no qual o erro ocorreu.
        /// </summary>
        private LexSymbol Symbol { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Tipo de erro: Preprocessamento, léxico, sintático, semântico
        /// </summary>
        public CompilationErrorType Type { get; private set; }

        /// <summary>
        /// Mensagem de erro
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Linha na qual o erro ocorreu.
        /// </summary>
        public int Line { get { return Symbol.Line; } }

        /// <summary>
        /// Coluna na qual o erro ocorreu.
        /// </summary>
        public int Column { get { return Symbol.Column; } }

        /// <summary>
        /// Texto do símbolo no qual o erro ocorreu.
        /// </summary>
        public string SymbolText { get { return Symbol.Text; } }

        #endregion

        #region Methods

        public override string ToString()
        {
            return "[LINE: {0}, COL: {1}] - [TYPE: {2}] - [MESSAGE: {3}]".FormatWith(Symbol.Line, Symbol.Column, Type, Message);
        }

        #endregion
    }
}
