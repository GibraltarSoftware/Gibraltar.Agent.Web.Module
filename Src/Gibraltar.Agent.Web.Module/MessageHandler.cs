using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Gibraltar.Agent.Web.Module.Models;
using Newtonsoft.Json;

namespace Gibraltar.Agent.Web.Module
{
    public class MessageHandler
    {
        private Regex urlRegex = new Regex("/[Gg]ibraltar/([Ll]og|[Ee]xception)(?!/.)");
        private JavaScriptLogger _javaScriptLogger;

        public JavaScriptLogger JavaScriptLogger
        {
            get
            {
                if (_javaScriptLogger == null)
                {
                    _javaScriptLogger = new JavaScriptLogger();    
                }

                return _javaScriptLogger;
            } 
            set { _javaScriptLogger = value; }
        }

        public void HandleRequest(HttpContextBase context)
        {
            var matched = urlRegex.Match(context.Request.Url.LocalPath);

            if (matched.Success)
            {
                switch (matched.Groups[1].Value.ToLowerInvariant())
                {
                    case "log":
                        LogMessage(context);
                        break;

                    case "exception":
                        LogException(context);
                        break;
                }
            }
        }

        public void LogMessage(HttpContextBase context)
        {

            string body;

            context.Request.InputStream.Position = 0;
            using (var streamReader = new StreamReader(context.Request.InputStream))
            {
                body = streamReader.ReadToEnd();
            }

            var message = JsonConvert.DeserializeObject<LogDetails>(body, new JsonSerializerSettings());

            JavaScriptLogger.LogMessage(message);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.End();            
        }

        public void LogException(HttpContextBase context)
        {
            string body;

            context.Request.InputStream.Position = 0;
            using (var streamReader = new StreamReader(context.Request.InputStream))
            {
                body = streamReader.ReadToEnd();
            }

            var message = JsonConvert.DeserializeObject<JavaScriptError>(body, new JsonSerializerSettings());

            JavaScriptLogger.LogException(message);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.End();               
        }
    }

    public class JavaScriptLogger
    {
        public virtual void LogMessage(LogDetails toLog)
        {
            
        }

        public virtual void LogException(JavaScriptError error)
        {
            
        }
    }
}