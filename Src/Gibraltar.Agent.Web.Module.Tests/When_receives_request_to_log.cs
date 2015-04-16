using System;
using ExpectedObjects;
using Gibraltar.Agent.Web.Module.Models;
using NSubstitute;
using NUnit.Framework;

namespace Gibraltar.Agent.Web.Module.Tests
{
    [TestFixture]
    public class When_receives_request_to_log: TestBase
    {

        [Test]
        public void Should_only_handle_POST([Values("GET", "PUT", "DELETE", "PATCH", "HEAD")] string method)
        {
            _httpRequest.Url.Returns(new Uri("http://www.test.com/gibraltar/log"));
            _httpRequest.HttpMethod.Returns(method);

            _target.HandleRequest(_httpContext);

            Assert.That(_httpResponse.StatusCode, Is.EqualTo(0));        
        }

        [Test]
        public void Should_return_200([Values("http://www.test.com/gibraltar/log",
                                              "http://www.test.com/Gibraltar/log",
                                              "http://www.test.com/gibraltar/Log",
                                              "http://www.test.com/Gibraltar/Log",
                                              "http://www.test.com/gibraltar/log/")] string url)
        {
            
            CallWithRequestBody("{\"Category\":\"test\"}",url);

            Assert.That(_httpResponse.StatusCode, Is.EqualTo(200));            
        }

        [Test]
        public void Should_call_logger()
        {
            var fakeLogger = Substitute.For<JavaScriptLogger>();

            _target.JavaScriptLogger = fakeLogger;

            CallWithRequestBody("{\"Severity\":8,\"Category\":\"test\",\"Caption\":\"test logs message\"}", "http://wwww.test.com/gibraltar/log");

            fakeLogger.Received().LogMessage(Arg.Any<LogDetails>());
        }


        [Test]
        public void Should_pass_expected_object_to_log()
        {

            var fakeLogger = Substitute.For<JavaScriptLogger>();

            _target.JavaScriptLogger = fakeLogger;

            CallWithRequestBody("{\"Severity\":8,\"Category\":\"test\",\"Caption\":\"test logs message\"}", "http://wwww.test.com/gibraltar/log");

            var expected = new LogDetails
            {
                Severity = LogMessageSeverity.Information,
                Category = "test",
                Caption = "test logs message"
            }.ToExpectedObject();

            // ReSharper disable once SuspiciousTypeConversion.Global
            fakeLogger.Received().LogMessage(Arg.Is<LogDetails>(x => expected.Equals(x)));
        }
    }
}