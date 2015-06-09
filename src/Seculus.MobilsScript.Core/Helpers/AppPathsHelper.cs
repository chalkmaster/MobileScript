using System;
using System.IO;

namespace Seculus.MobileScript.Core.Helpers
{
    /// <summary>
    /// Contém os caminhos da aplicação.
    /// </summary>
    public static class AppPathsHelper
    {
        #region Constants

        private const string JsonConfigFileName = "app.json.config";
        private const string ModulesDirectoryName = "modules";
        private const string ModuleAssembliesDirectoryName = "assemblies";
        private const string ModuleResourcesDirectoryName = "resources";

        #endregion

        #region Public Methods

        /// <summary>
        /// Retorna o caminho do arquivo de configuração da aplicação.
        /// </summary>
        /// <param name="rootPath">Caminho base do deploy do AppPlat.</param>
        /// <returns>Caminho do arquivo de configuração.</returns>
        public static string GetJsonConfigPath(string rootPath)
        {
            if (String.IsNullOrEmpty(rootPath))
            {
                rootPath = InferRootPath();
            }
            return Path.Combine(rootPath, JsonConfigFileName);
        }

        /// <summary>
        /// Retorna o caminho da pasta raiz dos módulos.
        /// </summary>
        /// <param name="rootPath">Caminho base do deploy do AppPlat.</param>
        /// <returns>Caminho do diretório raiz onde ficam os módulos.</returns>
        public static string GetModulesPath(string rootPath)
        {
            if (String.IsNullOrEmpty(rootPath))
            {
                rootPath = InferRootPath();
            }
            return Path.Combine(rootPath, ModulesDirectoryName);
        }

        /// <summary>
        /// Retorna o caminho das assemblies de um módulo.
        /// </summary>
        /// <param name="modulePath">Caminho do módulo.</param>
        /// <returns>Caminho das assemblies do módulo.</returns>
        public static string GetModuleAssembliesPath(string modulePath)
        {
            Check.Argument.IsNotNullOrEmpty(modulePath, "modulePath");

            var path = Path.Combine(modulePath, ModuleAssembliesDirectoryName);
            if (Directory.Exists(path))
            {
                return path;
            }
            return modulePath;
        }

        /// <summary>
        /// Retorna o caminho das assemblies de um módulo.
        /// </summary>
        /// <param name="modulePath">Caminho do módulo.</param>
        /// <returns>Caminho dos recursos do módulo ou NULL se o caminho não existir.</returns>
        public static string GetModuleResourcesPath(string modulePath)
        {
            Check.Argument.IsNotNullOrEmpty(modulePath, "modulePath");

            var path = Path.Combine(modulePath, ModuleResourcesDirectoryName);
            if (Directory.Exists(path))
            {
                return path;
            }
            return null;
        }

        #endregion

        #region Private Methods

        private static string InferRootPath()
        {
            var configFileFullPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            return Path.GetDirectoryName(configFileFullPath);
        }

        #endregion
    }
}
