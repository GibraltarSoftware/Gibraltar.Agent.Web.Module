using System.Web;
using Loupe.Agent.Web.Module.Infrastructure;

namespace Loupe.Agent.Web.Module.Handlers
{
    public class HeaderHandler
    {
        private const string clientHeaderName = "loupe-agent-sessionId";

        public void HandleRequest(HttpContextBase context)
        {
            if (context.Request.InterestedInRequest())
            {
                CreateContextItem(context);

                if (HasHeader(context))
                {
                    AddValueToContext(context);
                }
            }
        }

        private bool HasHeader(HttpContextBase context)
        {
            return context.Request.Headers[clientHeaderName] != null;
        }

        private void CreateContextItem(HttpContextBase context)
        {
            context.Items.Add(Constants.AgentSessionId, "");
        }

        private void AddValueToContext(HttpContextBase context)
        {
            context.Items[Constants.AgentSessionId] = context.Request.Headers[clientHeaderName];
        }
    }
}