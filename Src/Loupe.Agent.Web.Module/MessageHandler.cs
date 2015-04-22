using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Gibraltar.Agent;
using Loupe.Agent.Web.Module.Models;
using Newtonsoft.Json;

namespace Loupe.Agent.Web.Module
{
    public class MessageHandler
    {
        private const string LogSystem = "Loupe";
        private const string Category = "Loupe.Internal";
        private readonly Regex _urlRegex = new Regex("/[Ll]oupe/[Ll]og(?!/.)", RegexOptions.Compiled);
        private JavaScriptLogger _javaScriptLogger;

        public JavaScriptLogger JavaScriptLogger
        {
            get { return _javaScriptLogger ?? (_javaScriptLogger = new JavaScriptLogger()); }
            set { _javaScriptLogger = value; }
        }


        public void HandleRequest(HttpContextBase context)
        {
            var urlMatch = _urlRegex.Match(context.Request.Url.LocalPath);

            if (urlMatch.Success && RequestIsValid(context))
            {
                LogMessage(context);
            }
        }

        private bool RequestIsValid(HttpContextBase context)
        {
            if (!ValidateMethod(context)) return false;

            if (!ValidateInputStream(context)) return false;

            return true;
        }

        private void ResponseHandled(HttpContextBase context, HttpStatusCode statusCode)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.End();
        }

        private bool ValidateInputStream(HttpContextBase context)
        {
            if (context.Request.InputStream.Length == 0)
            {
                // No request body return bad request
#if DEBUG
                Log.Write(LogMessageSeverity.Information, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Empty request body",
                    "Request was received for but no body was included in the POST");
#endif

                ResponseHandled(context, HttpStatusCode.BadRequest);

                return false;
            }

            if (context.Request.InputStream.Length > 2048)
            {
#if DEBUG
                Log.Write(LogMessageSeverity.Information, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Request body exceeds limit",
                    "Request was received but the body included exceeded the size limit of 2k and was {0}", SizeSuffix(context.Request.InputStream.Length));   
#endif       
                ResponseHandled(context, HttpStatusCode.BadRequest);                
            }

            return true;
        }

        private bool ValidateMethod(HttpContextBase context)
        {
            if (context.Request.HttpMethod.ToLowerInvariant() != "post")
            {
                // We have received a request which is specifically for us but on a method we don't support.
                // Record that this has happened and then let the request carry on and host application
                // deal with an invalid request
#if DEBUG
                Log.Write(LogMessageSeverity.Warning, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Invalid HttpMethod",
                    "Received request but was a {0} rather than a POST", context.Request.HttpMethod);
#endif
                return false;
            }

            return true;
        }

        private void LogMessage(HttpContextBase context)
        {
            var logRequest = GetMessageFromRequestBody(context);

            if (logRequest != null)
            {
                logRequest.User = context.User;

                JavaScriptLogger.Log(logRequest);

                ResponseHandled(context, HttpStatusCode.OK);
            }
        }

        private LogRequest GetMessageFromRequestBody(HttpContextBase context) 
        {
            var requestBody = ReadInputStream(context);

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return null;
            }
            
            return DeserializeBody(context, requestBody);
        }

        private string ReadInputStream(HttpContextBase context)
        {
            string body = null;

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
#if DEBUG
                Log.Write(LogMessageSeverity.Error, LogSystem, 0, ex, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Error reading input stream",
                    "An exception occured whilst attempting to read the request input stream to enable deserialization of the data");
#endif
            }

            return body;
        }

        private LogRequest DeserializeBody(HttpContextBase context, string body)
        {
            LogRequest requestBody = null;

            try
            {
                requestBody = JsonConvert.DeserializeObject<LogRequest>(body, new JsonSerializerSettings());
            }
            catch (Exception ex)
            {           
#if DEBUG
                Log.Write(LogMessageSeverity.Error, LogSystem, 0, ex, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context,body), Category, "Error deserializing request body",
                    "An exception occured whilst attempting to deserialize the request body");
#endif
            }

            return requestBody;
        }

        private string CreateStandardRequestDetailXml(HttpContextBase context, string requestBody = null)
        {
            const string detailsFormat = "<UserAgent>{0}</UserAgent><ContentType>{1}</ContentType><ContentLength>{2}</ContentLength><IsLocal>{3}</IsLocal><IsSecureConnection>{4}</IsSecureConnection><UserHostAddress>{5}</UserHostAddress><UserHostName>{6}</UserHostName>";

            var details = new StringBuilder();

            details.Append("<Request>");

            details.AppendFormat(detailsFormat, context.Request.Browser.Browser, context.Request.ContentType, context.Request.ContentLength, context.Request.IsLocal,
                context.Request.IsSecureConnection, context.Request.UserHostAddress, context.Request.UserHostName);
            
            if (requestBody != null)
            {
                details.Append("<RequestBody>" + requestBody + "</RequestBody>");
            }

            details.Append("</Request>");

            return details.ToString();
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
}