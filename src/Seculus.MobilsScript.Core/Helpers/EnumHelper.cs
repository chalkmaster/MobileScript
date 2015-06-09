using System;
using System.Collections.Generic;
using System.Linq;

namespace Seculus.MobileScript.Core.Helpers
{
    /// <summary>
    /// Métodos auxiliares para enum.
    /// </summary>
    public class EnumHelper
    {
        /// <summary>
        /// Retorna o nome de exibição de um item de um enum desde que o item esteja decorado com o atributo <see cref="EnumDisplayNameAttribute"/>.
        /// </summary>
        /// <param name="e">Item do enum.</param>
        /// <returns>Display name do item do enum.</returns>
        public static string GetDisplayName(Enum e)
        {
            var enumDisplayNameAttribute = (EnumDisplayNameAttribute)e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(EnumDisplayNameAttribute), false).First();
            return enumDisplayNameAttribute.Name;
        }

        /// <summary>
        /// Nome de exibição de todos os itens do enum, desde que eles estejam decordados com o atributo <see cref="EnumDisplayNameAttribute"/>. 
        /// A chave do dicionário é o Nome do item do enum.
        /// O valor, é o display name.
        /// </summary>
        /// <returns>Dicionário com o nome do item da enum (key) e o display name (valor).</returns>
        public static Dictionary<Enum, string> GetDisplayNameDictionary<T>()
        {
            return GetDisplayNameDictionary(typeof(T));
        }

        /// <summary>
        /// Nome de exibição de todos os itens do enum, desde que eles estejam decordados com o atributo <see cref="EnumDisplayNameAttribute"/>. 
        /// A chave do dicionário é o Nome do item do enum.
        /// O valor, é o display name.
        /// </summary>
        /// <param name="enumType">Tipo do enum</param>
        /// <returns>Dicionário com o nome do item da enum (key) e o display name (valor).</returns>
        public static Dictionary<Enum, string> GetDisplayNameDictionary(Type enumType)
        {
            var enumDisplayNames = new Dictionary<Enum, string>();

            var query = enumType.GetFields().Where(e => e.IsSpecialName == false);

            foreach (var enumField in query)
            {
                var enumDisplayNameAttribute =
                    (EnumDisplayNameAttribute)
                    enumField.GetCustomAttributes(typeof(EnumDisplayNameAttribute), false).First();
                enumDisplayNames.Add((Enum)enumField.GetValue(null), enumDisplayNameAttribute.Name);
            }

            return enumDisplayNames;
        }
    }
}
