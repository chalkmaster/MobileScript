namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Categoria do símbolo.
    /// </summary>
    public enum SymbolKind
    {
        /// <summary>
        /// Contante
        /// </summary>
        Constant = 0,

        /// <summary>
        /// Tipo primitivo
        /// </summary>
        PrimitiveType = 1,

        /// <summary>
        /// Tipo vetor
        /// </summary>
        VectorType = 2,

        /// <summary>
        /// Variável
        /// </summary>
        Variable = 3,

        /// <summary>
        /// Parâmetro
        /// </summary>
        Parameter = 4,

        /// <summary>
        /// Function
        /// </summary>
        Function = 5
    }
}
