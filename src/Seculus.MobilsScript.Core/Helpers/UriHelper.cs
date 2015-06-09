using System;

namespace Seculus.MobileScript.Core.Helpers
{
    public class UriHelper
    {
        public static Uri Combine(string address, string path)
        {
            const string delimiter = @"/";

            return (address.EndsWith(delimiter) || path.StartsWith(delimiter))
                       ? new Uri(String.Concat(address, path))
                       : new Uri(String.Concat(address, delimiter, path));            
        }

        public static Uri Combine(Uri uri, string path)
        {
            return Combine(uri.ToString(), path);
        }    
    }
}