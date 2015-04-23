using System;
using System.Web;

namespace Loupe.Agent.Web.Module
{
    public class Logging:IHttpModule
    {
        private MessageHandler _messageHandler;
        private CookieHandler _cookieHandler;

        public void Init(HttpApplication application)
        {
            _messageHandler = new MessageHandler();
            _cookieHandler = new CookieHandler();
            application.PostAuthenticateRequest += OnPostAuthenticateRequest;
            application.BeginRequest += application_BeginRequest;
        }

        void application_BeginRequest(object sender, EventArgs e)
        {
            _cookieHandler.HandleRequest(new HttpContextWrapper(((HttpApplication)sender).Context));
        }

        private void OnPostAuthenticateRequest(object sender, EventArgs e)
        {
            _messageHandler.HandleRequest(new HttpContextWrapper(((HttpApplication)sender).Context)); ;
        }

        // Implemented as part of IHttpModule
        // but we don't use it
        public void Dispose() { }
    }
}
