namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    public enum CompilationErrorType
    {
        /// <summary>
        /// Erro de preprocessamento
        /// </summary>
        PreprocessorError = 0,

        /// <summary>
        /// Erro léxico
        /// </summary>
        LexError = 1,

        /// <summary>
        /// Erro sintático
        /// </summary>
        SyntaxError = 2,

        /// <summary>
        /// Erro semântico
        /// </summary>
        SemanticError = 3
    }
}
