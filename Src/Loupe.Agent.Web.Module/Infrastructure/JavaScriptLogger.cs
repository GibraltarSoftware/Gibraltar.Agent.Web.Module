using Gibraltar.Agent;
using Loupe.Agent.Web.Module.DetailBuilders;
using Loupe.Agent.Web.Module.Models;

namespace Loupe.Agent.Web.Module.Infrastructure
{
    public class JavaScriptLogger
    {
        /// <summary>
        /// Handles logging of the message to Loupe
        /// </summary>
        /// <param name="logRequest">A request received from the agent to log information</param>
        public virtual void Log(LogRequest logRequest)
        {
            var detailsBlockBuilder = new LogMessageBlockBuilder(logRequest);
            var sourceProvider = new JavaScriptSourceProvider();

            foreach (var logMessage in logRequest.LogMessages)
            {
                var jsException = CreateJavaScriptException(logMessage);

                var detailsBlock = detailsBlockBuilder.Build(logMessage);

                var messageSource = sourceProvider.ProcessMessage(logMessage);

                Gibraltar.Agent.Log.Write(logMessage.Severity,
                                          "Loupe",
                                           messageSource,
                                           "",
                                          jsException,
                                          LogWriteMode.Queued,
                                          detailsBlock,
                                          logMessage.Category,
                                          logMessage.Caption,
                                          logMessage.Description,
                                          logMessage.Parameters);
            }
        }

        private static JavaScriptException CreateJavaScriptException(LogMessage logMessage)
        {
            JavaScriptException jsException = null;
            if (logMessage.Exception != null && !logMessage.Exception.IsEmpty())
            {
                jsException = new JavaScriptException(logMessage.Exception.Message, logMessage.Exception.StackTrace);
            }
            return jsException;
        }
    }
}