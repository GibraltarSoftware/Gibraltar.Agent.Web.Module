using System;
using Gibraltar.Agent;
using Loupe.Agent.Web.Module.Models;

namespace Loupe.Agent.Web.Module.DetailBuilders
{
    public class LogMessageBlockBuilder:DetailsBuilderBase
    {
        private readonly LogRequest _request;
        private string _clientDetails;
        private string _sessionDetails;

        public LogMessageBlockBuilder(LogRequest request)
        {
            _request = request;
            _clientDetails = null;
        }

        public string Build(LogMessage logMessage)
        {
            if (_clientDetails == null)
            {
                _clientDetails = CreateClientDetailString(_request);
            }

            DetailBuilder.Clear();

            DetailBuilder.Append("<Details>");
            DetailBuilder.Append(SessionDetailString(logMessage));
            DetailBuilder.Append(TimeStampDetailString(logMessage));
            DetailBuilder.Append(SequenceDetailString(logMessage));
            DetailBuilder.Append(_clientDetails);
            DetailBuilder.Append(CreateMethodSourceInfoString(logMessage));
            DetailBuilder.Append(UserSuppliedDetails(logMessage));
            DetailBuilder.Append("</Details>");

            return DetailBuilder.ToString();
        }

        private string CreateClientDetailString(LogRequest logRequest)
        {
            if (logRequest.Session != null && logRequest.Session.Client != null)
            {
                return ObjectToXmlString(logRequest.Session.Client);
            }

            return null;
        }

        private static string SessionDetailString(LogMessage message)
        {
            string sessionDetails = null;
                
            if (!string.IsNullOrWhiteSpace(message .SessionId))
            {
                sessionDetails = "<SessionId>" + message.SessionId + "</SessionId>";
            }

            if (!string.IsNullOrWhiteSpace(message.AgentSessionId))
            {
                sessionDetails = sessionDetails + "<AgentSessionId>" + message.AgentSessionId + "</AgentSessionId>";
            }

            return sessionDetails;
        }

        private static string TimeStampDetailString(LogMessage logMessage)
        {
            string timeStamp = null;

            if (!logMessage.TimeStamp.Equals(new DateTimeOffset()))
            {
                timeStamp = string.Format("<TimeStamp>{0}</TimeStamp>", logMessage.TimeStamp);
            }

            return timeStamp;
        }

        private static string SequenceDetailString(LogMessage logMessage)
        {
            return string.Format("<Sequence>{0}</Sequence>", logMessage.Sequence);
        }

        private string CreateMethodSourceInfoString(LogMessage logMessage)
        {
            if (logMessage.MethodSourceInfo != null && !logMessage.MethodSourceInfo.IsEmpty())
            {
                return ObjectToXmlString(logMessage.MethodSourceInfo);
            }

            return null;
        }

        private string UserSuppliedDetails(LogMessage logMessage)
        {
            if (string.IsNullOrWhiteSpace(logMessage.Details))
            {
                return null;
            }

            try
            {
                return JObjectToXmlString(logMessage.Details);
            }
            catch
            {
                // deatils are not json or we cannot convert to
                // xml so we will simply add the details to the block
                // as a string
            }

            return "<UserSupplied>" + logMessage.Details + "</UserSupplied>";
        }
         
    }
}