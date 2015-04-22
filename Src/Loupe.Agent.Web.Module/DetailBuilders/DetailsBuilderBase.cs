using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Gibraltar.Agent;
using Newtonsoft.Json;

namespace Loupe.Agent.Web.Module.DetailBuilders
{
    public class DetailsBuilderBase
    {
        private readonly XmlSerializerNamespaces _xmlNamespaces;
        private readonly XmlWriterSettings _xmlWriterSettings;
        protected StringBuilder DetailBuilder;

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

            try
            {
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter, _xmlWriterSettings))
                {
                    xmlSerializer.Serialize(xmlTextWriter, detailsObject, _xmlNamespaces);
                    xmlTextWriter.Flush();
                    return stringWriter.GetStringBuilder().ToString();
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                    Log.Write(LogMessageSeverity.Error, "Loupe", 0, ex, LogWriteMode.Queued,
                        "", "Loupe.Internal", "Failed to serialize object",
                        "Exception thrown whilst attempting to serialize {0} for this request", typeof(T).Name);
#endif
            }

            return null;
        }
    }
}