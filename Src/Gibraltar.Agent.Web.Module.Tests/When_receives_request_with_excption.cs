using System;
using System.IO;
using System.Web;
using Gibraltar.Agent.Web.Module.Models;
using NSubstitute;
using NUnit.Framework;

namespace Gibraltar.Agent.Web.Module.Tests
{
    [TestFixture]
    public class When_receives_request_with_excption
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
        public void Should_return_200([Values("http://www.test.com/gibraltar/exception",
                                              "http://www.test.com/Gibraltar/exception",
                                              "http://www.test.com/gibraltar/Exception",
                                              "http://www.test.com/Gibraltar/Exception",
                                              "http://www.test.com/gibraltar/exception/")] string url)
        {
            _httpRequest.Url.Returns(new Uri(url));

            _target.HandleRequest(_httpContext);

            Assert.That(_httpResponse.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public void Should_call_logger()
        {
            var requestBody =
                "{\"Category\":\"JavaScript\",\"Message\":\"Error: Test Error\",\"Url\":\"http://localhost:3371/specs/When_logging_exception.js\",\"StackTrace\":[\"createError/<@http://localhost:3371/specs/When_logging_exception.js:37:19\"],\"Cause\":\"\",\"Line\":37,\"Column\":18,\"Details\":\"\"}";

            var fakeLogger = Substitute.For<JavaScriptLogger>();

            _target.JavaScriptLogger = fakeLogger;

            using (var fakeInputStream = new MemoryStream())
            using (var writer = new StreamWriter(fakeInputStream))
            {
                writer.Write(requestBody);
                writer.Flush();


                _httpRequest.Url.Returns(new Uri("http://wwww.test.com/gibraltar/exception"));
                _httpRequest.InputStream.Returns(fakeInputStream);

                _target.HandleRequest(_httpContext);
            }

            fakeLogger.Received().LogException(Arg.Any<JavaScriptError>());
        }
    }
}