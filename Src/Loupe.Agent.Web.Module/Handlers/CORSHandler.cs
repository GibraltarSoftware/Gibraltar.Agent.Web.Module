using System.Net;
using System.Web;
using Gibraltar.Agent;

namespace Loupe.Agent.Web.Module.Handlers
{
    public class CORSHandler
    {
        private readonly UrlCheck _urlCheck;
        private HostCORSConfiguration _configruation;


        /// <summary>
        /// Allows tests to inject mock class for testing purposes
        /// </summary>
        internal HostCORSConfiguration Configuration
        {
            get { return _configruation ?? (_configruation = new HostCORSConfiguration()); }
            set { _configruation = value; }
        }

        public CORSHandler()
        {
            _urlCheck = new UrlCheck();
        }

        public bool HandleRequest(HttpContextBase context)
        {
            bool handled = false;

            if (IsCrossOriginRequest(context) && _urlCheck.IsLoupeUrl(context))
            {
                try
                {
                    switch (context.Request.HttpMethod)
                    {
                        case "OPTIONS":
                            CreateOptionsResponse(context);
                            handled = true;
                            break;

                        case "POST":
                            AddHeadersToPost(context);
                            // don't flag as handled as we need the request
                            // to continue through the pipeline
                            break;

                        default:
                            MethodNotSupportedResponse(context);
                            handled = true;
                            break;
                    }
                }
                catch (System.Exception ex)
                {
#if DEBUG
                    Log.Write(LogMessageSeverity.Critical, "Loupe", 0, ex, LogWriteMode.Queued,
                        null, "Loupe.Internal", "Exception attempting to handle CORS request",
                        "Exception occured trying to handle the {0} request of a CORS request", context.Request.HttpMethod);
#endif
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    handled = true;
                }
            }

            return handled;
        }

        private void CreateOptionsResponse(HttpContextBase context)
        {
            AddAllowHeaders(context);

            AddAllowOrigin(context);

            AddAllowMethods(context);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        private void AddHeadersToPost(HttpContextBase context)
        {
            AddAllowOrigin(context);
        }

        private void MethodNotSupportedResponse(HttpContextBase context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.MethodNotAllowed;
        }

        private void AddAllowMethods(HttpContextBase context)
        {
            if (!Configuration.HasAllowMethods())
            {
                context.Response.AddHeader("Access-Control-Allow-Methods", "POST");
            }
        }

        private void AddAllowOrigin(HttpContextBase context)
        {
            if (!Configuration.HasAllowOrigin())
            {
                context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            }
        }

        private void AddAllowHeaders(HttpContextBase context)
        {
            var requestHeader = context.Request.Headers.Get("Access-Control-Request-Headers");

            if (requestHeader != null & !Configuration.HasAllowHeaders())
            {
                context.Response.AddHeader("Access-Control-Allow-Headers", requestHeader);
            }
        }

        private bool IsCrossOriginRequest(HttpContextBase context)
        {
            return context.Request.Headers.Get("Origin") != null;
        }
    }
}