namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Representa o nível em que o símbolo é declarado no programa.
    /// </summary>
    public enum SymbolLevel
    {
        /// <summary>
        /// Símbolo pré-definido.
        /// </summary>
        PreDefined = 0,

        /// <summary>
        /// Símbolo global (declarado no programa).
        /// </summary>
        Global = 1,

        /// <summary>
        /// Símbolo local (declarado numa função).
        /// </summary>
        Local = 2
    }
}
