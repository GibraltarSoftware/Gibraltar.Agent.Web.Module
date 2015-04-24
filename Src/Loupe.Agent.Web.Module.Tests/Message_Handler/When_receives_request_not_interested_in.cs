using System;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.Message_Handler
{
    [TestFixture]
    public class When_receives_unrelated_request:TestBase
    {

        [Test]
        public void Should_pass_request_on([Values("http://www.test.com/", 
                                                   "http://www.test.com/Gibraltar", 
                                                   "http://www.test.com/Gibraltar/log/things",
                                                   "http://www.test.com/gibraltar/data")] string url)
        {
            HttpRequest.Url.Returns(new Uri(url));

            Target.HandleRequest(HttpContext);

            Assert.That(HttpResponse.StatusCode, Is.EqualTo(0));
        }
    }
}