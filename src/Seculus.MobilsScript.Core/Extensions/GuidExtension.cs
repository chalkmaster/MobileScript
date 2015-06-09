using System;

namespace Seculus.MobileScript.Core.Extensions
{
    public static class GuidExtension
    {
        #region Public Methods

        public static string Normalize(this Guid guid)
        {
            return guid.ToString().Replace("-", "").ToUpper();
        }

        #endregion
    }
}
