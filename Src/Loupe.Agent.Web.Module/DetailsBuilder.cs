using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Loupe.Agent.Web.Module.Models;
using Newtonsoft.Json;

namespace Loupe.Agent.Web.Module
{
    public class DetailsBuilder
    {
        private readonly LogRequest _request;

        private readonly XmlSerializerNamespaces _xmlNamespaces;
        private readonly XmlWriterSettings _xmlWriterSettings;
        private string _clientDetails;
        private readonly StringBuilder _detailsBuilder;

        public DetailsBuilder(LogRequest request)
        {
            _request = request;
            _detailsBuilder = new StringBuilder();
            _clientDetails = null;
            _xmlNamespaces = new XmlSerializerNamespaces(new []{XmlQualifiedName.Empty});
            _xmlWriterSettings = new XmlWriterSettings();
            _xmlWriterSettings.OmitXmlDeclaration = true;
            
        }

        public string Build(LogMessage logMessage)
        {
            if (_clientDetails == null)
            {
                _clientDetails = CreateClientDetailString(_request);                
            }

            _detailsBuilder.Clear();

            _detailsBuilder.Append("<Details>");
            _detailsBuilder.Append(TimeStampAndSequenceString(logMessage));
            _detailsBuilder.Append(_clientDetails);
            _detailsBuilder.Append(CreateMethodSourceInfoString(logMessage));
            _detailsBuilder.Append(UserSuppliedDetails(logMessage));
            _detailsBuilder.Append("</Details>");

            return _detailsBuilder.ToString();
        }

        private string CreateClientDetailString(LogRequest logRequest)
        {
            if (logRequest.Session != null && logRequest.Session.Client != null)
            {
                return ObjectToXmlString(logRequest.Session.Client);
            }

            return "<ClientDetails></ClientDetails>";
        }

        private string TimeStampAndSequenceString(LogMessage logMessage)
        {
            return string.Format("<TimeStamp>{0}</TimeStamp><Sequence>{1}</Sequence>", logMessage.TimeStamp,
                logMessage.Sequence);
        }

        private string CreateMethodSourceInfoString(LogMessage logMessage)
        {
            if (logMessage.MethodSourceInfo != null && !logMessage.MethodSourceInfo.IsEmpty())
            {
                return ObjectToXmlString(logMessage.MethodSourceInfo);
            }

            return "<MethodSourceInfo></MethodSourceInfo>";
        }

        private string UserSuppliedDetails(LogMessage logMessage)
        {
            if (string.IsNullOrWhiteSpace(logMessage.Details))
            {
                return "<UserSupplied></UserSupplied>";
            }

            try
            {
                return JObjectToXmlString(logMessage.Details);
            }
            catch
            {
                // deatils are not json or we cannot convert to
                // xml so we will simply add the details to the block
                // as a string
            }

            return "<UserSupplied>" + logMessage.Details + "</UserSupplied>";
        }

        private string JObjectToXmlString(string details)
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

        private string ObjectToXmlString<T>(T detailsObject)
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