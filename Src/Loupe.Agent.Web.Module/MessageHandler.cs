using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Gibraltar.Agent;
using Loupe.Agent.Web.Module.DetailBuilders;
using Loupe.Agent.Web.Module.Models;
using Newtonsoft.Json;
using Exception = System.Exception;

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
            if (UrlMatch(context) && RequestIsValid(context))
            {
                LogMessage(context);
            }
        }

        private bool UrlMatch(HttpContextBase context)
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
                Log.Write(LogMessageSeverity.Error, LogSystem, 0, ex, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Unable to match regex",
                    "Exception thrown when attempting to match the regex against the local path");
#endif
            }

            return false;
        }

        private bool RequestIsValid(HttpContextBase context)
        {
            if (!ValidateMethod(context)) return false;

            if (!ValidateInputStream(context)) return false;

            return true;
        }

        private static void ResponseHandled(HttpContextBase context, HttpStatusCode statusCode, string userDescription = null)
        {
            if (!string.IsNullOrWhiteSpace(userDescription))
            {
                context.Response.StatusDescription = userDescription;
            }
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

                ResponseHandled(context, HttpStatusCode.BadRequest, "Empty request body");

                return false;
            }

            if (context.Request.InputStream.Length > 2048)
            {
#if DEBUG
                Log.Write(LogMessageSeverity.Information, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Request body exceeds limit",
                    "Request was received but the body included exceeded the size limit of 2k and was {0}", SizeSuffix(context.Request.InputStream.Length));   
#endif       
                ResponseHandled(context, HttpStatusCode.RequestEntityTooLarge, "Request body exceeds size limit");                
            }

            return true;
        }

        private static bool ValidateMethod(HttpContextBase context)
        {
            if (context.Request.HttpMethod.ToUpperInvariant() != "POST")
            {
                // We have received a request which is specifically for us but on a method we don't support.
                // Record that this has happened and then let the request carry on and host application
                // deal with an invalid request
#if DEBUG
                Log.Write(LogMessageSeverity.Warning, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Invalid HttpMethod",
                    "Received request but was a {0} rather than a POST", context.Request.HttpMethod);
#endif

                ResponseHandled(context, HttpStatusCode.MethodNotAllowed, context.Request.HttpMethod + " not allowed. Use POST");
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

                try
                {
                    JavaScriptLogger.Log(logRequest);

                    ResponseHandled(context, HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Log.Write(LogMessageSeverity.Error, LogSystem, 0, ex, LogWriteMode.Queued,
                        CreateStandardRequestDetailXml(context), Category, "Error writing log to Loupe",
                        "An exception occured whilst attempting to record the LogRequest in Loupe");                                  
#endif
                    ResponseHandled(context,HttpStatusCode.InternalServerError,"Unable to record log request");
                }
            }
        }

        private static LogRequest GetMessageFromRequestBody(HttpContextBase context) 
        {
            var requestBody = ReadInputStream(context);

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return null;
            }
            
            return DeserializeBody(context, requestBody);
        }

        private static string ReadInputStream(HttpContextBase context)
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

        private static LogRequest DeserializeBody(HttpContextBase context, string body)
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

        private static string CreateStandardRequestDetailXml(HttpContextBase context, string requestBody = null)
        {
            var builder = new RequestBlockBuilder(context, requestBody);

            return builder.Build();
        }

        private readonly string[] _sizeSuffixes = { "bytes", "KB", "MB", "GB" };
        private string SizeSuffix(long value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, _sizeSuffixes[mag]);
        }
    }
}