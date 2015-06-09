using System;
using Seculus.MobileScript.Core.Helpers;

namespace Seculus.MobileScript.Core.Extensions
{
    /// <summary>
    /// Extensões de enums
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Retorna o nome de exibição de um item de um enum desde que o item esteja decorado com o atributo <see cref="EnumDisplayNameAttribute"/>.
        /// </summary>
        /// <param name="e">Item do enum.</param>
        /// <returns>Display name do item do enum.</returns>
        public static string GetDisplayName(this Enum e)
        {
            return EnumHelper.GetDisplayName(e);
        }
    }
}
