using System;
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
        private const string LogSystem = "Loupe";
        private const string Category = "Loupe.Internal";
        private readonly Regex _urlRegex = new Regex("/[Gg]ibraltar/([Ll]og|[Ee]xception)(?!/.)", RegexOptions.Compiled);
        private JavaScriptLogger _javaScriptLogger;

        public JavaScriptLogger JavaScriptLogger
        {
            get { return _javaScriptLogger ?? (_javaScriptLogger = new JavaScriptLogger()); }
            set { _javaScriptLogger = value; }
        }

        public void HandleRequest(HttpContextBase context)
        {

            var matched = _urlRegex.Match(context.Request.Url.LocalPath);

            if (matched.Success)
            {
                var requestFor = matched.Groups[1].Value.ToLowerInvariant();

                if (context.Request.HttpMethod.ToLowerInvariant() != "post")
                {
                    // We have received a request which is specifically for us but on a method we don't support.
                    // Record that this has happened and then let the request carry on and host application
                    // deal with an invalid request

                    Log.Write(LogMessageSeverity.Warning,LogSystem,0,null,LogWriteMode.Queued,CreateStandardRequestDetailXml(context),Category,"Invalid HttpMethod", "Received request for {0} but was not a {1} rather than a POST",requestFor, context.Request.HttpMethod);
                    return;
                }
                
                if (context.Request.InputStream.Length == 0)
                {
                    // No request body, log it and return InternalServerError to JS agent
                    Log.Write(LogMessageSeverity.Information, LogSystem, 0, null, LogWriteMode.Queued, CreateStandardRequestDetailXml(context), Category, "Empty request body", "Request was received for {0} but no body was included in the POST", requestFor);
                    context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    context.Response.End();

                    return;
                }


                switch (requestFor)
                {
                    case "log":
                        LogMessage(context);
                        break;

                    case "exception":
                        LogException(context);
                        break;

                    default:
                        Log.Write(LogMessageSeverity.Information, LogSystem, 0, null, LogWriteMode.Queued, CreateStandardRequestDetailXml(context),Category,"Unknown function requested", "Request received for {0} which is not supported by the module",requestFor);
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.End(); 
                        return;
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.End();         
            }
        }


        public void LogMessage(HttpContextBase context)
        {
            var message = GetBody<LogDetails>(context);

            if (message != null)
            {
                JavaScriptLogger.LogMessage(message);
            }

        }

        public void LogException(HttpContextBase context)
        {
            var message = GetBody<JavaScriptError>(context);

            if (message != null)
            {
                JavaScriptLogger.LogException(message);
            }
        }


        public T GetBody<T>(HttpContextBase context) where T:class 
        {
            var body = ReadInputStream<T>(context);

            if (!string.IsNullOrWhiteSpace(body))
            {
                return DeserializeBody<T>(context, body);
            }

            return null;
        }



        private string ReadInputStream<T>(HttpContextBase context)
        {
            string body = "";

            try
            {
                context.Request.InputStream.Position = 0;
                using (var streamReader = new StreamReader(context.Request.InputStream))
                {
                    body = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogMessageSeverity.Error, LogSystem, 0, ex, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Error reading input stream",
                    "An exception occured whilst attempting to read the request input stream to enable deserialization of the data");
            }
            return body;
        }

        private T DeserializeBody<T>(HttpContextBase context, string body) where T: class
        {
            T requestBody;

            try
            {
                requestBody = JsonConvert.DeserializeObject<T>(body, new JsonSerializerSettings());
            }
            catch (Exception ex)
            {
                Log.Write(LogMessageSeverity.Error, LogSystem, 0, ex, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Error deserializing request body",
                    "An exception occured whilst attempting to deserialize the request body to {0}", typeof(T).Name);
                requestBody = null;
            }
            return requestBody;
        }

        private string CreateStandardRequestDetailXml(HttpContextBase context)
        {
            string detailsFormat =
                "<Request><ContentType>{0}</ContentType><IsLocal>{1}</IsLocal><IsSecureConnection>{2}</IsSecureConnection><UserHostAddress>{3}</UserHostAddress><UserHostName>{4}</UserHostName><UserName>{5}</UserName></Request>";

            string details = string.Format(detailsFormat, context.Request.ContentType, context.Request.IsLocal,
                context.Request.IsSecureConnection, context.Request.UserHostAddress, context.Request.UserHostName,
                context.User.Identity.Name);
            return details;
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