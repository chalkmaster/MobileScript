using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Seculus.MobileScript.Core.Helpers
{
    /// <summary>
    /// Métodos auxiliares para tratar arquivos de recurso.
    /// </summary>
    public static class ResourcesHelper
    {
        #region Public Methods

        /// <summary>
        /// Retorna o valor de uma chave de recurso de forma destipada (reflection)
        /// </summary>
        /// <param name="resourceManagerProvider">Resrouce</param>
        /// <param name="resourceKey">Chave</param>
        /// <returns>Valor da chave do resource informado.</returns>
        public static string LookupResource(Type resourceManagerProvider, string resourceKey)
        {
            Check.Argument.IsNotNull(resourceManagerProvider, "resourceManagerProvider");
            Check.Argument.IsNotNullOrEmpty(resourceKey, "resourceKey");

            var property = resourceManagerProvider.GetProperty(resourceKey, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(string))
            {
                return (string)property.GetValue(null, null);
            }
            return resourceKey;
        }

        /// <summary>
        /// Converte um resx para dicionário
        /// </summary>
        /// <param name="resourceAssembly">Assembly que contém o resx</param>
        /// <param name="resourceName">Nome do arquivo resx sem a extensão e sem o namespace</param>
        /// <returns>Dicionário com todos os itens do resx</returns>
        public static Dictionary<string, object> GetResourceAsDictionary(Assembly resourceAssembly, string resourceName)
        {
            var resourceNamespace = resourceAssembly.GetName().Name;
            var resourceTypeName = string.Format("{0}.{1}Resources", resourceNamespace, resourceName);
            var resourceManager = new ResourceManager(resourceTypeName, resourceAssembly);

            try
            {
                var resourceSet = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
                return ConvertResourceToDictionary(resourceSet); ;
            }
            catch (MissingManifestResourceException)
            {
                return new Dictionary<string, object>();
            }
        }

        #endregion

        #region Private Methods

        private static Dictionary<string, object> ConvertResourceToDictionary(ResourceSet resourceSet)
        {
            var resourceSetDictionary = new Dictionary<string, object>();
            var dictionaryEnumerator = resourceSet.GetEnumerator();
            while (dictionaryEnumerator.MoveNext())
            {
                resourceSetDictionary.Add(dictionaryEnumerator.Key.ToString(), dictionaryEnumerator.Value);
            }

            return resourceSetDictionary;
        }

        #endregion


        /*
        //Based: http://yobriefca.se/blog/2011/03/29/serialising-net-resources-to-json-for-web-apps/
        //<script type="text/javascript">
        //    var Strings = <%= ResourceSerialiser.ToJson(typeof(Some.Resource.Strings)) %>
        //</script>
        #region JSON Serialisation
        /// <summary>
        /// Converts a resrouce type into an equivalent JSON object using the 
        /// current Culture
        /// </summary>
        /// <param name="resource">The resoruce type to serialise</param>
        /// <returns>A JSON string representation of the resource</returns>
        public static string ToJson(Type resource)
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            return ToJson(resource, culture);
        }

        /// <summary>
        /// Converts a resrouce type into an equivalent JSON object using the 
        /// culture derived from the language code passed in
        /// </summary>
        /// <param name="resource">The resoruce type to serialise</param>
        /// <param name="languageCode">The language code to derive the culture</param>
        /// <returns>A JSON string representation of the resource</returns>
        public static string ToJson(Type resource, string languageCode)
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(languageCode);
            return ToJson(resource, culture);
        }

        /// <summary>
        /// Converts a resrouce type into an equivalent JSON object
        /// </summary>
        /// <param name="resource">The resoruce type to serialise</param>
        /// <param name="culture">The culture to retrieve</param>
        /// <returns>A JSON string representation of the resource</returns>
        public static string ToJson(Type resource, CultureInfo culture)
        {
            Dictionary<string, string> dictionary = ResourceToDictionary(resource, culture);
            return JsonConvert.SerializeObject(dictionary);
        }

        #endregion

        /// <summary>
        /// Converts a resrouce type into a dictionary type while localising 
        /// the strings using the passed in culture
        /// </summary>
        /// <param name="resource">The resoruce type to serialise</param>
        /// <param name="culture">The culture to retrieve</param>
        /// <returns>A dictionary representation of the resource</returns>
        private static Dictionary<string, string> ResourceToDictionary(Type resource, CultureInfo culture)
        {
            ResourceManager rm = new ResourceManager(resource);
            PropertyInfo[] pis = resource.GetProperties(BindingFlags.Public | BindingFlags.Static);
            IEnumerable<KeyValuePair<string, string>> values =
                from pi in pis
                where pi.PropertyType == typeof(string)
                select new KeyValuePair<string, string>(
                    pi.Name,
                    rm.GetString(pi.Name, culture));
            Dictionary<string, string> dictionary = values.ToDictionary(k => k.Key, v => v.Value);

            return dictionary;
        }
        
        */
    }
}