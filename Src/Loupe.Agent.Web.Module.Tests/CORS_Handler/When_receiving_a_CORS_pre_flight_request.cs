using System;
using System.Collections.Specialized;
using System.Web;
using Loupe.Agent.Web.Module.Handlers;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.CORS_Handler
{
    [TestFixture]
    public class When_receiving_a_CORS_pre_flight_request
    {
        

        private CORSHandler Target;

        private HttpContextBase HttpContext;
        private HttpRequestBase HttpRequest;
        private HttpResponseBase HttpResponse;
        private HostCORSConfiguration _fakeConfig;

        private NameValueCollection httpResponseHeaders;

        [SetUp]
        public void SetUp()
        {

            HttpContext = Substitute.For<HttpContextBase>();
            HttpRequest = Substitute.For<HttpRequestBase>();
            HttpResponse = Substitute.For<HttpResponseBase>();
            _fakeConfig = Substitute.For<HostCORSConfiguration>();

            httpResponseHeaders = new NameValueCollection();

            HttpResponse.Headers.Returns(httpResponseHeaders);
            HttpResponse.When(x => x.AddHeader(Arg.Any<string>(), Arg.Any<string>())).Do(x =>
            {
                AddToResponseHeaders(x.Args()[0].ToString(), x.Args()[1].ToString());
            });

            HttpRequest.Headers.Returns(new NameValueCollection());
            HttpContext.Request.Returns(HttpRequest);
            HttpContext.Response.AddHeader(Arg.Any<string>(),Arg.Any<String>());
            HttpContext.Response.Returns(HttpResponse);

            Target = new CORSHandler();
            Target.Configruation = _fakeConfig;
        }

        private void AddToResponseHeaders(string name, string value)
        {
            httpResponseHeaders.Add(name, value);
        }

        [Test]
        public void Should_pass_request_on_if_not_for_loupe([Values("http://www.test.com/",
                                                                    "http://www.test.com/Gibraltar",
                                                                    "http://www.test.com/Gibraltar/log/things",
                                                                    "http://www.test.com/gibraltar/data")] string url)
        {
            HttpRequest.Url.Returns(new Uri(url));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.False);
            Assert.That(HttpResponse.StatusCode, Is.EqualTo(0));
        }

        [Test]
        public void Should_not_handle_if_no_origin_header()
        {
            HttpRequest.Url.Returns(new Uri("http://test.com/gibraltar/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.False);
            Assert.That(HttpResponse.StatusCode, Is.EqualTo(0));
        }

        [Test]
        public void Should_respond_with_appropriate_headers()
        {
            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin","http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);
            Assert.That(HttpResponse.Headers["Access-Control-Allow-Methods"],Is.EqualTo("POST"));
        }

        [Test]
        public void Should_include_allow_headers_if_present_on_request()
        {
            _fakeConfig.AllowHeaders().Returns(false);

            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");
            HttpRequest.Headers.Add("Access-Control-Request-Headers", "content-type");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);
            Assert.That(HttpResponse.Headers["Access-Control-Allow-Headers"], Is.EqualTo("content-type"));            
        }

        [Test]
        public void Should_not_add_CORS_header_if_in_customHeaders_in_config()
        {
            _fakeConfig.AllowOrigin().Returns(true);

            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);

            Assert.That(HttpResponse.Headers["Access-Control-Allow-Origin"], Is.Null);
        }

        [Test]
        public void Should_not_add_CORS_header_if_not_set_in_config()
        {
            _fakeConfig.AllowOrigin().Returns(false);

            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);

            Assert.That(HttpResponse.Headers["Access-Control-Allow-Origin"], Is.EqualTo("*"));
        }

        [Test]
        public void Should_not_add_allow_headers_if_set_in_config()
        {
            _fakeConfig.AllowHeaders().Returns(true);
            HttpRequest.HttpMethod.Returns("OPTIONS");
            HttpRequest.Headers.Add("Origin", "http://www.mysite.com/loupe/log");
            HttpRequest.Headers.Add("Access-Control-Request-Headers", "content-type");

            HttpRequest.Url.Returns(new Uri("http://test.com/loupe/log"));

            var actual = Target.HandleRequest(HttpContext);

            Assert.That(actual, Is.True);
            Assert.That(HttpResponse.Headers["Access-Control-Allow-Headers"], Is.Null);                  
        }
    } 
}