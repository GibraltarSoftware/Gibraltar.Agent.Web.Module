using System.Collections.Generic;
using System.Web;

namespace Loupe.Agent.Web.Module.Infrastructure
{
    public static class Extension
    {
        public static bool InterestedInRequest(this HttpRequestBase request)
        {
            List<string> extenstionWhiteList = new List<string> { ".html", ".htm", ".aspx", "" };

            return extenstionWhiteList.Contains(request.CurrentExecutionFilePathExtension) &&
                   !request.CurrentExecutionFilePath.Contains("__browserLink");
        }

    }
}