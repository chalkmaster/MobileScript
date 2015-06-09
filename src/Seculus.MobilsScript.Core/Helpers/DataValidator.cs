using System;
using System.Text.RegularExpressions;

namespace Seculus.MobileScript.Core.Helpers
{
    /// <summary>
    /// Classe estática responsável pela validação de dados.
    /// </summary>
    public static class DataValidator
    {
        #region Constants

        /// <summary>
        /// Expressão regular que valida e-mails.
        /// </summary>
        public const string EmailRegex = @"^([0-9a-zA-Z]+[-._+&])*[0-9a-zA-Z]+@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$";

        #endregion

        #region Static Fields

        private static readonly Regex EmailExpression = new Regex(EmailRegex, RegexOptions.Singleline | RegexOptions.Compiled);

        #endregion

        #region Static Methods

        /// <summary>
        /// Informa se uma string é um e-mail válido.
        /// </summary>
        /// <param name="maybeEmail">String que se deseja verifica se é um e-mail.</param>
        /// <returns>True se for um e-mail válido. Caso contrário, false.</returns>
        public static bool IsEmail(string maybeEmail)
        {
            return !String.IsNullOrEmpty(maybeEmail) && EmailExpression.IsMatch(maybeEmail);
        }

        #endregion
    }
}
