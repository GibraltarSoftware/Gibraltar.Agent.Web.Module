using System;
using System.Web;
using NSubstitute;
using NUnit.Framework;

namespace Gibraltar.Agent.Web.Module.Tests
{
    [TestFixture]
    public class When_receives_unrelated_request
    {

        private HttpContextBase _httpContext;
        private HttpRequestBase _httpRequest;
        private HttpResponseBase _httpResponse;
        private MessageHandler _target;
        private HttpApplication _app;

        [SetUp]
        public void SetUp()
        {
            _app = new HttpApplication();
            _target = new MessageHandler();

            _httpContext = Substitute.For<HttpContextBase>();
            _httpRequest = Substitute.For<HttpRequestBase>();
            _httpResponse = Substitute.For<HttpResponseBase>();

            _httpContext.Request.Returns(_httpRequest);
            _httpContext.Response.Returns(_httpResponse);
        }

        [Test]
        public void Should_pass_request_on([Values("http://www.test.com/", 
                                                   "http://www.test.com/Gibraltar", 
                                                   "http://www.test.com/Gibraltar/log/things")] string url)
        {
            _httpRequest.Url.Returns(new Uri(url));

            _target.HandleRequest(_httpContext);

            Assert.That(_httpResponse.StatusCode, Is.EqualTo(0));
        }
    }
}