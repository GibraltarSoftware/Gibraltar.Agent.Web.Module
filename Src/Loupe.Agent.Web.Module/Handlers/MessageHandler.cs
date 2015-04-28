using System;
using System.IO;
using System.Net;
using System.Web;
using Gibraltar.Agent;
using Loupe.Agent.Web.Module.DetailBuilders;
using Loupe.Agent.Web.Module.Infrastructure;
using Loupe.Agent.Web.Module.Models;
using Newtonsoft.Json;
using Exception = System.Exception;

namespace Loupe.Agent.Web.Module.Handlers
{
    public class MessageHandler
    {
        private const string LogSystem = "Loupe";
        private const string Category = "Loupe.Internal";
        private JavaScriptLogger _javaScriptLogger;
        private readonly UrlCheck _urlCheck;

        /// <summary>
        /// Allows tests to inject mock class for testing purposes
        /// </summary>
        internal JavaScriptLogger JavaScriptLogger
        {
            get { return _javaScriptLogger ?? (_javaScriptLogger = new JavaScriptLogger()); }
            set { _javaScriptLogger = value; }
        }

        public MessageHandler()
        {
            _urlCheck = new UrlCheck();   
        }

        public bool HandleRequest(HttpContextBase context)
        {
            var handled = false;

            try
            {
                if (_urlCheck.IsLoupeUrl(context))
                {
                    // if the request is for loupe then we want to 
                    // indicate its handled so the actual module
                    // completes the request when this method 
                    // exits
                    handled = true;

                    if (RequestIsValid(context))
                    {
                        LogMessage(context);                        
                    }

                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Log.Write(LogMessageSeverity.Critical, LogSystem, 0, ex, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "Exception attempting to process message",
                    "Exception caught in top level catch block, this should have be caught by error handler specific to the part of the request processing that failed.");
#endif
            }

            return handled;
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
                AddSessionId(context, logRequest);

                try
                {
                    JavaScriptLogger.Log(logRequest);

                    ResponseHandled(context, HttpStatusCode.NoContent);
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

        private void AddSessionId(HttpContextBase context, LogRequest logRequest)
        {
            var sessionCookie = context.Request.Cookies.Get("LoupeSessionId");

            context.Items.Add("LoupeSessionId", "");

            if (logRequest.Session == null)
            {
                logRequest.Session = new ClientSession();
            }

            if (sessionCookie == null && string.IsNullOrWhiteSpace(logRequest.Session.SessionId))
            {
#if DEBUG
                Log.Write(LogMessageSeverity.Warning, LogSystem, 0, null, LogWriteMode.Queued,
                    CreateStandardRequestDetailXml(context), Category, "No session Id",
                    "No session cookie for Loupe or explicit session Id found on the request, not able to set session Id");
#endif                
                return;
            }


            if (sessionCookie != null && string.IsNullOrWhiteSpace(logRequest.Session.SessionId))
            {
                logRequest.Session.SessionId = sessionCookie.Value;    
            }

            // store the id in the HttpContext so available to agents in 
            // actual code base for logging
            context.Items["LoupeSessionId"]= logRequest.Session.SessionId;
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