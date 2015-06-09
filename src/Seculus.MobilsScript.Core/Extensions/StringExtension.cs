using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Seculus.MobileScript.Core.Helpers;

namespace Seculus.MobileScript.Core.Extensions
{
    /// <summary>
    /// Contém os métodos de extensão da classe System.String
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Formata uma string. Equivalente ao método String.Format().
        /// </summary>
        /// <param name="target">Um formato de string.</param>
        /// <param name="args">Argumentos para substituir na string.</param>
        /// <returns>String formatada.</returns>
        public static string FormatWith(this string target, params object[] args)
        {
            Check.Argument.IsNotNullOrEmpty(target, "target");
            return String.Format(target, args);
        }

        /// <summary>
        /// Trunca uma string ao tamanho requisitado.
        /// </summary>
        /// <param name="target">String a ser truncada.</param>
        /// <param name="maxSize">Tamanho máximo da string.</param>
        /// <returns>String truncada (ou a string original caso ela seja menor que o tamanho máximo).</returns>
        public static string Truncate(this string target, int maxSize)
        {
            Check.Argument.IsNotNullOrEmpty(target, "target");

            return (target.Length > maxSize) ? target.Substring(0, maxSize) : target;
        }

        /// <summary>
        /// Informa se uma string é um e-mail válido.
        /// </summary>
        /// <param name="target">Possivelmente e-mail.</param>
        /// <returns>True se for um e-mail válido. Caso contrário, false.</returns>
        public static bool IsEmail(this string target)
        {
            return DataValidator.IsEmail(target);
        }

        /// <summary>
        /// Verifica se uma cadeia de caracteres está contida na string
        /// </summary>
        /// <param name="target">string principal</param>
        /// <param name="stringToFind">cadeia a ser verificada se existe na string principal</param>
        /// <param name="insensitive">não checar maiúsculas e minúsculas (neste caso "JOÃO" e "joão" são iguais)</param>
        /// <param name="ignoreDiacriticals">ignorar acentuação (neste caso "João" e "Joao" são iguais)</param>
        /// <returns>verdadeiro casa a cadeia de caracteres seja encontrada na string principal</returns>
        public static bool IsLike(this string target, string stringToFind, bool insensitive = true, bool ignoreDiacriticals = true)
        {
            if (ignoreDiacriticals)
            {
                target = target.RemoveAccents();
                stringToFind = stringToFind.RemoveAccents();
            } 
            if (insensitive)
            {
                target = target.ToLowerInvariant();
                stringToFind = stringToFind.ToLowerInvariant();
            }
            return target.Contains(stringToFind);
        }

        /// <summary>
        /// Remove acentos de uma string
        /// </summary>
        /// <param name="input">string a ser removida os acentos</param>
        /// <returns>string sem acentos</returns>
        public static string RemoveAccents(this string input)
        {
            string normalized = input.Normalize(NormalizationForm.FormKD);
            Encoding removal = Encoding.GetEncoding(
                                    Encoding.ASCII.CodePage,
                                    new EncoderReplacementFallback(""),
                                    new DecoderReplacementFallback("")
                                );
            var bytes = removal.GetBytes(normalized);
            return Encoding.ASCII.GetString(bytes);
        }
        
        /// <summary>
        /// Validação de CNPJ
        /// </summary>
        /// <param name="target">CNPJ a ser validado</param>
        /// <returns>True caso a string seja um CNPJ válido</returns>
        [DebuggerStepThrough]
        public static bool IsCnpj(this string target)
        {
            target = target.Trim();

            if (string.IsNullOrEmpty(target))
            {
                return false;
            }

            var invalidsCnpj = new[] { 
                "00000000000000", "11111111111111", "22222222222222", "33333333333333", 
                "44444444444444", "55555555555555", "66666666666666", "77777777777777",
                "88888888888888", "99999999999999" };

            if (invalidsCnpj.Any(invalidCnpj => invalidCnpj.Equals(target)))
            {
                return false;
            }

            var firstMultiplier = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var secondMultiplier = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var sum = 0;


            if (!Regex.Match(target, "[0-9]{14}").Success)
            {
                return false;
            }

            var helper = target.Substring(0, 12);

            for (var i = 0; i < 12; i++)
            {
                sum += int.Parse(helper[i].ToString(CultureInfo.InvariantCulture)) * firstMultiplier[i];
            }

            var mod = sum % 11;
            mod = mod < 2 ? 0 : 11 - mod;

            var digit = mod.ToString(CultureInfo.InvariantCulture);

            helper += digit;

            sum = 0;
            for (var i = 0; i < 13; i++)
            {
                sum += int.Parse(helper[i].ToString(CultureInfo.InvariantCulture)) * secondMultiplier[i];
            }

            mod = sum % 11;
            mod = mod < 2 ? 0 : 11 - mod;

            digit += mod.ToString(CultureInfo.InvariantCulture);

            return target.EndsWith(digit);
        }

        /// <summary>
        /// Retorna a chave MD5 da string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string MD5(this string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Transforma a primeira letra em maiúscula.
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>String alterada</returns>
        public static string UpperCaseFirstChar(this string str)
        {
            if (str.Length == 0) return str;

            var firstCharUpperCased = str[0].ToUpper();
            if (str.Length > 1)
            {
                return firstCharUpperCased + str.Substring(1);
            }
            else
            {
                return firstCharUpperCased.ToString(Thread.CurrentThread.CurrentCulture);
            }
        }
    }
}
