using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Xml.Linq;

namespace Loupe.Agent.Web.Module.Handlers
{
    public class HostCORSConfiguration
    {
        const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        private const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

        private Dictionary<string, string> GlobalHeaders { get; set; }


        public virtual bool HasAllowOrigin()
        {
            if(GlobalHeaders == null) LoadConfigValues();

            return GlobalHeaders.ContainsKey(AccessControlAllowOrigin);
        }

        public virtual bool HasAllowHeaders()
        {
            if (GlobalHeaders == null) LoadConfigValues();

            return GlobalHeaders.ContainsKey(AccessControlAllowHeaders);
        }

        private void LoadConfigValues()
        {
            var doc = LoadXml;

            var headers = GetElementByPath(doc, "httpProtocol.customHeaders");

            if (headers != null)
            {
                foreach (var node in headers.Descendants().Where(x => ((string) x.Attribute("name")).Contains("Access-Control"))
                    )
                {
                    foreach (var attribute in node.Attributes())
                    {
                        GlobalHeaders.Add(attribute.Name.LocalName, attribute.Value);
                    }
                }
            }
        }

        private XDocument LoadXml
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