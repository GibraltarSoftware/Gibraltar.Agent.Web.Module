using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Loupe.Agent.Web.Module.DetailBuilders
{
    public class DetailsBuilderBase
    {
        private readonly XmlSerializerNamespaces _xmlNamespaces;
        private readonly XmlWriterSettings _xmlWriterSettings;
        protected readonly StringBuilder DetailBuilder;

        protected DetailsBuilderBase()
        {
            DetailBuilder = new StringBuilder();
            _xmlNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            _xmlWriterSettings = new XmlWriterSettings();
            _xmlWriterSettings.OmitXmlDeclaration = true;
        }

        protected string JObjectToXmlString(string details)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter, _xmlWriterSettings))
            {
                var detailsDoc = JsonConvert.DeserializeXmlNode(details, "UserSupplied");
                detailsDoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        protected string ObjectToXmlString<T>(T detailsObject)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));


            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter, _xmlWriterSettings))
            {
                xmlSerializer.Serialize(xmlTextWriter, detailsObject, _xmlNamespaces);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }
    }
}