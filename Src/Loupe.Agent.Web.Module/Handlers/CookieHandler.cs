using System;
using System.Web;
using Loupe.Agent.Web.Module.Infrastructure;

namespace Loupe.Agent.Web.Module.Handlers
{
    public class CookieHandler
    {
        public void HandleRequest(HttpContextBase context)
        {
            if (InterestedInRequest(context.Request))
            {
                if (CookieDoesNotExist(context.Request))
                {
                    AddSessionCookie(context);
                }

                AddContextItem(context);
            }
        }

        private void AddContextItem(HttpContextBase context)
        {
            var sessionId = context.Request.Cookies[Constants.SessionId].Value;
            context.Items.Add(Constants.SessionId, sessionId);
        }

        private void AddSessionCookie(HttpContextBase context)
        {
            var loupeCookie = new HttpCookie(Constants.SessionId);
            loupeCookie.HttpOnly = true;
            loupeCookie.Value = Guid.NewGuid().ToString();
            
            context.Request.Cookies.Add(loupeCookie);
            context.Response.Cookies.Add(loupeCookie);            
        }

        private bool CookieDoesNotExist(HttpRequestBase request)
        {
            return request.Cookies[Constants.SessionId] == null;
        }


        private bool InterestedInRequest(HttpRequestBase request)
        {
            // cookies not supported for CORS
            if (request.Headers.Get("Origin") != null)
            {
                return false;
            }

            return request.InterestedInRequest();
        }

    }
}