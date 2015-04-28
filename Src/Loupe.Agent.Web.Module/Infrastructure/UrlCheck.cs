using System;
using System.Text.RegularExpressions;
using System.Web;
using Gibraltar.Agent;

namespace Loupe.Agent.Web.Module.Infrastructure
{
    public class UrlCheck
    {
        private readonly Regex _urlRegex = new Regex("/[Ll]oupe/[Ll]og(?!/.)", RegexOptions.Compiled);

        public bool IsLoupeUrl(HttpContextBase context)
        {
            try
            {
                var urlMatch = _urlRegex.Match(context.Request.Url.LocalPath);

                return urlMatch.Success;
            }
            catch (Exception ex)
            {
                // for whatever reason the match has thrown an error, in 
                // normal circumnstances this should not happen but to ensure
                // we do not cause problems elsewhere we will swall this 
                // exception and let the request continue through the pipeline
#if DEBUG
                Log.Write(LogMessageSeverity.Error, "Loupe", 0, ex, LogWriteMode.Queued, null, "Loupe.Internal", "Unable to match regex",
                    "Exception thrown when attempting to match the regex against the local path");
#endif
            }

            return false;
        }
    }
}