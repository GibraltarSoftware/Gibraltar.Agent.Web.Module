using System;
using System.Web;
using Loupe.Agent.Web.Module.Handlers;

namespace Loupe.Agent.Web.Module
{
    public class Logging:IHttpModule
    {
        private CORSHandler _corsHandler;
        private MessageHandler _messageHandler;
        private CookieHandler _cookieHandler;
        private HeaderHandler _headerHandler;

        public void Init(HttpApplication application)
        {
            _corsHandler = new CORSHandler();
            _messageHandler = new MessageHandler();
            _cookieHandler = new CookieHandler();
            _headerHandler = new HeaderHandler();
            application.PostAuthenticateRequest += OnPostAuthenticateRequest;
            application.BeginRequest += application_BeginRequest;
        }

        void application_BeginRequest(object sender, EventArgs e)
        {
            var context = new HttpContextWrapper(((HttpApplication)sender).Context);

            if (_corsHandler.HandleRequest(context))
            {
                ((HttpApplication)sender).CompleteRequest();
            }
            else
            {
                _cookieHandler.HandleRequest(context);
                _headerHandler.HandleRequest(context);
            }
        }

        private void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication) sender;

            var handled = _messageHandler.HandleRequest(new HttpContextWrapper(application.Context));

            if (handled)
            {
               application.CompleteRequest();
            }
        }

        // Implemented as part of IHttpModule
        // but we don't use it
        public void Dispose() { }
    }
}
