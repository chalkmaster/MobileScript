using System.Threading;

namespace Seculus.MobileScript.Core.Extensions
{
    public static class CharExtension
    {
        public static char ToUpper(this char chr)
        {
            return chr.ToString(Thread.CurrentThread.CurrentCulture).ToUpper()[0];
        }
    }
}
