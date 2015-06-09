using System;
using System.Globalization;

namespace Seculus.MobileScript.Core.Helpers
{
    public static class StringHelper
    {
        public static string ToEnUsString(this double @value)
        {
            return Math.Round(@value, 2, MidpointRounding.ToEven).ToString(CultureInfo.GetCultureInfo("en-US"));
        }

        public static string ToEnUsString(this long @value)
        {
            return @value.ToString(CultureInfo.GetCultureInfo("en-US"));
        }
    }
}
