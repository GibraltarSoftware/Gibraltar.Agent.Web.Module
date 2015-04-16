using System;
using System.Web;

namespace Gibraltar.Agent.Web.Module
{
    public class Logging:IHttpModule
    {
        private MessageHandler _messageHandler;

        public void Init(HttpApplication application)
        {
            _messageHandler = new MessageHandler();
            application.PostAuthenticateRequest += OnPostAuthenticateRequest;
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
