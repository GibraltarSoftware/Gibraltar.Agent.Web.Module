using System.Web;

namespace Loupe.Agent.Web.Module.DetailBuilders
{
    public class RequestBlockBuilder:DetailsBuilderBase
    {
        private readonly HttpContextBase _context;
        private readonly string _requestBody;

        public RequestBlockBuilder(HttpContextBase context, string requestBody)
        {
            _context = context;
            _requestBody = requestBody;
            
        }

        public string Build()
        {
            const string detailsFormat = "<UserAgent>{0}</UserAgent><ContentType>{1}</ContentType><ContentLength>{2}</ContentLength><IsLocal>{3}</IsLocal><IsSecureConnection>{4}</IsSecureConnection><UserHostAddress>{5}</UserHostAddress><UserHostName>{6}</UserHostName>";

            DetailBuilder.Clear();

            DetailBuilder.Append("<Request>");

            DetailBuilder.AppendFormat(detailsFormat, _context.Request.Browser.Browser, _context.Request.ContentType, _context.Request.ContentLength, _context.Request.IsLocal,
                _context.Request.IsSecureConnection, _context.Request.UserHostAddress, _context.Request.UserHostName);

            if (_requestBody != null)
            {
                DetailBuilder.Append("<RequestBody>" + _requestBody + "</RequestBody>");
            }

            DetailBuilder.Append("</Request>");

            return DetailBuilder.ToString();
        }         
    }
}