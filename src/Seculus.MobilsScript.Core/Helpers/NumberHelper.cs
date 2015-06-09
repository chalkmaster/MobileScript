using System;
using System.Globalization;

namespace Seculus.MobileScript.Core.Helpers
{
    public static class NumberHelper
    {
        /// <summary>
        /// Arredonda o numero na quantidade de casas decimais indicada
        /// </summary>
        public static double Round(this double number, int precision = -1, MidpointRounding midpointRounding = MidpointRounding.ToEven)
        {
            if (precision >= 0) 
                return Math.Round(number, precision, midpointRounding);

            var numberFormat = CultureInfo.CurrentUICulture.NumberFormat;
            precision = numberFormat.CurrencyDecimalDigits;
            return Math.Round(number, precision, midpointRounding);
        }
    }
}
