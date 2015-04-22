using Loupe.Agent.Web.Module.Models;

namespace Loupe.Agent.Web.Module.DetailBuilders
{
    public class LogMessageBlockBuilder:DetailsBuilderBase
    {
        private readonly LogRequest _request;
        private string _clientDetails;

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
            DetailBuilder.Append(TimeStampAndSequenceString(logMessage));
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

            return "<ClientDetails></ClientDetails>";
        }

        private static string TimeStampAndSequenceString(LogMessage logMessage)
        {
            return string.Format("<TimeStamp>{0}</TimeStamp><Sequence>{1}</Sequence>", logMessage.TimeStamp,
                logMessage.Sequence);
        }

        private string CreateMethodSourceInfoString(LogMessage logMessage)
        {
            if (logMessage.MethodSourceInfo != null && !logMessage.MethodSourceInfo.IsEmpty())
            {
                return ObjectToXmlString(logMessage.MethodSourceInfo);
            }

            return "<MethodSourceInfo></MethodSourceInfo>";
        }

        private string UserSuppliedDetails(LogMessage logMessage)
        {
            if (string.IsNullOrWhiteSpace(logMessage.Details))
            {
                return "<UserSupplied></UserSupplied>";
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