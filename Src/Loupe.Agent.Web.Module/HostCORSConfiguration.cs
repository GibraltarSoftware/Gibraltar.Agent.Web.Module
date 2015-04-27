using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Xml.Linq;

namespace Loupe.Agent.Web.Module
{
    public class HostCORSConfiguration
    {
        const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        const string AccessControlAllowMethods = "Access-Control-Allow-Methods";

        private Dictionary<string, string> GlobalHeaders { get; set; }


        public virtual bool HasAllowOrigin()
        {
            return HasHeader(AccessControlAllowOrigin);
        }

        public virtual bool HasAllowHeaders()
        {
            return HasHeader(AccessControlAllowHeaders);
        }

        public virtual bool HasAllowMethods()
        {
            return HasHeader(AccessControlAllowMethods);
        }

        private bool HasHeader(string header)
        {
            if (GlobalHeaders == null) LoadConfigValues();

            return GlobalHeaders.ContainsKey(header);            
        }

        private void LoadConfigValues()
        {
            GlobalHeaders = new Dictionary<string, string>();

            var doc = LoadXmlForSystemWebServerSection;

            var headers = GetElementByPath(doc, "httpProtocol.customHeaders");

            PopulateHeaders(headers);
        }

        private void PopulateHeaders(XElement headers)
        {
            if (headers != null)
            {
                foreach (var node in headers.Descendants()
                                            .Where(x => x.HasAttributes && ((string) x.Attribute("name")).Contains("Access-Control")))
                {
                    var headerName = node.Attribute("name").Value;
                    var headerValue = node.Attribute("value").Value;

                    GlobalHeaders.Add(headerName, headerValue);
                }
            }
        }

        private XDocument LoadXmlForSystemWebServerSection
        {
            get
            {
                var configuration = WebConfigurationManager.OpenWebConfiguration("~");

                var webServerConfig = configuration.GetSection("system.webServer");

                var xml = webServerConfig.SectionInformation.GetRawXml();


                var doc = XDocument.Parse(xml);
                return doc;
            }
        }

        public XElement GetElementByPath(XDocument document, string path)
        {
            var elementNames = path.Split('.');
            var currentElement = document.Root;

            foreach (var elementName in elementNames)
            {
                var element = GetElement(currentElement, elementName);
                if (element == null)
                {
                    return null;
                }

                currentElement = element;
            }

            return currentElement;
        }

        public XElement GetElement(XElement parentElement, string name)
        {
            return parentElement.Descendants(name).FirstOrDefault();
        }

    }
}