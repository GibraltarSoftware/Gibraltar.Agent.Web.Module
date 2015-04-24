using System.Net;
using System.Web;

namespace Loupe.Agent.Web.Module.Handlers
{
    public class CORSHandler
    {
        private readonly UrlCheck _urlCheck;

        public CORSHandler()
        {
            _urlCheck = new UrlCheck();
        }

        public bool HandleRequest(HttpContextBase context)
        {
            bool handled = false;


            if (IsCrossOriginRequest(context) && _urlCheck.IsLoupeUrl(context))
            {
                if (context.Request.HttpMethod == "OPTIONS")
                {
                    CreateResponse(context);

                    handled = true;
                }
            }

            return handled;
        }

        private static void CreateResponse(HttpContextBase context)
        {
            var requestHeader = context.Request.Headers.Get("Access-Control-Request-Headers");

            if (requestHeader != null)
            {
                context.Response.AddHeader("Access-Control-Allow-Headers", requestHeader);
            }

            context.Response.StatusCode = (int) HttpStatusCode.OK;
            context.Response.AddHeader("Access-Control-Allow-Methods", "POST");
        }


        private bool IsCrossOriginRequest(HttpContextBase context)
        {
            return context.Request.Headers.Get("Origin") != null;
        }
    }
}