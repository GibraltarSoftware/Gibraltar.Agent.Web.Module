using System;
using System.Collections.Generic;
using System.Web;

namespace Loupe.Agent.Web.Module
{
    public class CookieHandler
    {
        readonly List<string> extenstionWhiteList = new List<string>{".html", ".htm",".aspx" ,""}; 

        private const string CookieName = "Loupe";

        public void HandleRequest(HttpContextBase context)
        {
            if (InterestedInRequest(context.Request))
            {

                if (CookieDoesNotExist(context.Request))
                {
                    AddSessionCookie(context);
                }
            }
        }

        private void AddSessionCookie(HttpContextBase context)
        {
            var loupeCookie = new HttpCookie(CookieName);
            loupeCookie.HttpOnly = true;
            loupeCookie.Value = Guid.NewGuid().ToString();
            
            context.Request.Cookies.Add(loupeCookie);
            context.Response.Cookies.Add(loupeCookie);            
        }

        private bool CookieDoesNotExist(HttpRequestBase request)
        {
            return request.Cookies[CookieName] == null;
        }


        private bool InterestedInRequest(HttpRequestBase request)
        {
            return extenstionWhiteList.Contains(request.CurrentExecutionFilePathExtension) &&
                   !request.CurrentExecutionFilePath.Contains("__browserLink");
        }
    }
}