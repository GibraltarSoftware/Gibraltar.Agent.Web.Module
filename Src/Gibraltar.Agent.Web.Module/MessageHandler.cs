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

                if (!RequestIsValid(context, requestFor))
                {
                    return;
                }

                if (requestFor == "log")
                {
                    LogMessage(context);
                }
                
                if(requestFor == "exception")
                {
                    LogException(context);
                }
            }
        }

        private bool RequestIsValid(HttpContextBase context, string requestFor)
        {
            if (!ValidateMethod(context, requestFor)) return false;

            if (!ValidateInputStream(context, requestFor)) return false;

            return true;
        }

        private bool ValidateInputStream(HttpContextBase context, string requestFor)
        {
            if (context.Request.InputStream.Length == 0)
            {
                // No request body, log it and return InternalServerError to JS agent
                Log.Write(LogMessageSeverity.Information, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Empty request body",
                    "Request was received for {0} but no body was included in the POST", requestFor);
                ResponseHandled(context, HttpStatusCode.BadRequest);

                return false;
            }

            if (context.Request.InputStream.Length > 2048)
            {
                Log.Write(LogMessageSeverity.Information, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Request body exceeds limit",
                    "Request was received for {0} but the body included exceeded the size limit of 2k and was {1}", requestFor, SizeSuffix(context.Request.InputStream.Length));
                ResponseHandled(context, HttpStatusCode.BadRequest);                
            }

            return true;
        }

        private bool ValidateMethod(HttpContextBase context, string requestFor)
        {
            if (context.Request.HttpMethod.ToLowerInvariant() != "post")
            {
                // We have received a request which is specifically for us but on a method we don't support.
                // Record that this has happened and then let the request carry on and host application
                // deal with an invalid request

                Log.Write(LogMessageSeverity.Warning, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Invalid HttpMethod",
                    "Received request for {0} but was not a {1} rather than a POST", requestFor, context.Request.HttpMethod);
                return false;
            }
            return true;
        }

        private void LogMessage(HttpContextBase context)
        {
            var message = GetBody<LogDetails>(context);

            if (message != null)
            {
                JavaScriptLogger.LogMessage(message);

                ResponseHandled(context, HttpStatusCode.OK);
            }
        }

        private void LogException(HttpContextBase context)
        {
            var message = GetBody<JavaScriptError>(context);

            if (message != null)
            {
                JavaScriptLogger.LogException(message);
                
                ResponseHandled(context, HttpStatusCode.OK);
            }
        }

        private void ResponseHandled(HttpContextBase context, HttpStatusCode statusCode)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.End();
        }

        private T GetBody<T>(HttpContextBase context) where T:class 
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


         readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB" };
         string SizeSuffix(long value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
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