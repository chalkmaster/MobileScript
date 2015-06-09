using System;

namespace Seculus.MobileScript.Core
{
    /// <summary>
    /// Permite atribuir um nome (human readable) para um item de um enum.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : Attribute
    {
        #region Fields

        private readonly string _name;
        private readonly Type _resourceManagerProvider;
        private readonly string _resourceKey;

        #endregion

        #region Constructors

        public EnumDisplayNameAttribute(string name)
        {
            _name = name;
        }

        public EnumDisplayNameAttribute(Type resourceManagerProvider, string resourceKey)
        {
            _name = null;
            _resourceManagerProvider = resourceManagerProvider;
            _resourceKey = resourceKey;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Nome do item do enum.
        /// </summary>
        public string Name
        {
            get
            {
                if (String.IsNullOrEmpty(_name))
                {
                    return _resourceKey;
                }
                else
                {
                    return _name;
                }
            }
        }

        #endregion
    }
}
