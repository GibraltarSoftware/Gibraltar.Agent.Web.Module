using System.Net;
using System.Web;

namespace Loupe.Agent.Web.Module.Handlers
{
    public class CORSHandler
    {
        private readonly UrlCheck _urlCheck;
        private HostCORSConfiguration _configruation;


        /// <summary>
        /// Allows tests to inject mock class for testing purposes
        /// </summary>
        internal HostCORSConfiguration Configruation
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

                if (context.Request.HttpMethod == "OPTIONS")
                {
                    CreateResponse(context);

                    handled = true;
                }
            }

            return handled;
        }

        private void CreateResponse(HttpContextBase context)
        {
            AddAllowHeaders(context);

            AddAllowOrigin(context);

            AddAllowMethods(context);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }

        private static void AddAllowMethods(HttpContextBase context)
        {
            context.Response.AddHeader("Access-Control-Allow-Methods", "POST");
        }

        private void AddAllowOrigin(HttpContextBase context)
        {
            if (!_configruation.HasAllowOrigin())
            {
                context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            }
        }

        private void AddAllowHeaders(HttpContextBase context)
        {
            var requestHeader = context.Request.Headers.Get("Access-Control-Request-Headers");

            if (requestHeader != null & !_configruation.HasAllowHeaders())
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