namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine
{
    /// <summary>
    /// Tipo do operand
    /// </summary>
    public enum DataType : byte
    {
        /// <summary>
        /// Representa um operando NULO.
        /// </summary>
        Null,

        /// <summary>
        /// Inteiro
        /// </summary>
        Int,

        /// <summary>
        /// String
        /// </summary>
        String,

        /// <summary>
        /// Caracter
        /// </summary>
        Char,

        /// <summary>
        /// Booleano
        /// </summary>
        Boolean,

        /// <summary>
        /// Ponto-flutuante
        /// </summary>
        Float
    }
}
