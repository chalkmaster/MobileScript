using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Seculus.MobileScript.Core.Helpers
{
    public static class XmlHelper
    {
        /// <summary>
        /// Gera o Scheema Xml do Tipo especificado
        /// </summary>
        public static XmlDocument GetScheema(this Type type)
        {
            var soapReflectionImporter = new SoapReflectionImporter();
            var xmlTypeMapping = soapReflectionImporter.ImportTypeMapping(type);
            var xmlSchemas = new XmlSchemas();
            var xmlSchema = new XmlSchema();
            xmlSchemas.Add(xmlSchema);
            var xmlSchemaExporter = new XmlSchemaExporter(xmlSchemas);
            xmlSchemaExporter.ExportTypeMapping(xmlTypeMapping);

            var stream = new MemoryStream();
            xmlSchema.Write(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(stream);

            return xmlDocument;
        }
    }
}
