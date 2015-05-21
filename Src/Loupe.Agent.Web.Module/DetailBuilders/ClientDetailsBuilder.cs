using Loupe.Agent.Web.Module.Models;

namespace Loupe.Agent.Web.Module.DetailBuilders
{
    public class ClientDetailsBuilder:DetailsBuilderBase
    {
        public string Build(LogRequest logRequest)
        {
            if (logRequest.Session != null && logRequest.Session.Client != null)
            {
                return ObjectToXmlString(logRequest.Session.Client);
            }

            return null;
        }         
    }
}