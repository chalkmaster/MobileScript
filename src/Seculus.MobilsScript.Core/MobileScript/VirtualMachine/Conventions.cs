namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine
{
    /// <summary>
    /// Convenções utilizadas na VM.
    /// </summary>
    public static class Conventions
    {
        /// <summary>
        /// String NULL e string vazio (usado nas listas)
        /// </summary>
        public const string NullString = "^??^"; // TODO: Change this!

        /// <summary>
        /// Caracter utilizado para identificar uma string.
        /// </summary>
        public const char StringChar = '"';

        /// <summary>
        /// Caracter utilizado para identificar um char.
        /// </summary>
        public const char CharChar = '\''; // argh!!!

        /// <summary>
        /// Caracter utilizado para identificar um bool.
        /// </summary>
        public const char BooleanChar = ':';

        /// <summary>
        /// Caracter utilizado para identificar um float.
        /// </summary>
        public const char FloatChar = '%';

        /// <summary>
        /// Caracter utilizado para identificar um inteiro.
        /// </summary>
        public const char IntChar = '#';
    }
}
