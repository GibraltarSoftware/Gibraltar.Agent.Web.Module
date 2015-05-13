using System.Web;
using Loupe.Agent.Web.Module.Infrastructure;

namespace Loupe.Agent.Web.Module.Handlers
{
    public class HeaderHandler
    {
        private const string clientHeaderName = "loupe-client-session";
        private const string loupeContextItemName = "LoupeClientSessionId";

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
            context.Items.Add(loupeContextItemName, null);
        }

        private void AddValueToContext(HttpContextBase context)
        {
            context.Items[loupeContextItemName] = context.Request.Headers[clientHeaderName];
        }
    }
}