using System;
using System.Collections.Generic;
using ExpectedObjects;
using Gibraltar.Agent.Web.Module.Models;
using NSubstitute;
using NUnit.Framework;

namespace Gibraltar.Agent.Web.Module.Tests
{
    [TestFixture]
    public class When_receives_request_with_excption:TestBase
    {

        [Test]
        public void Should_return_200([Values("http://www.test.com/gibraltar/exception",
                                              "http://www.test.com/Gibraltar/exception",
                                              "http://www.test.com/gibraltar/Exception",
                                              "http://www.test.com/Gibraltar/Exception",
                                              "http://www.test.com/gibraltar/exception/")] string url)
        {
            CallWithRequestBody("{\"Category\":\"JavaScript\"}",url);

            Assert.That(_httpResponse.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public void Should_call_logger()
        {
            var requestBody =
                "{\"Category\":\"JavaScript\",\"Message\":\"Error: Test Error\",\"Url\":\"http://www.test.com/app.js\",\"StackTrace\":[\"createError/<@http://www.test.com/app.js:37:19\"],\"Cause\":\"\",\"Line\":37,\"Column\":18,\"Details\":\"\"}";

            var fakeLogger = Substitute.For<JavaScriptLogger>();

            _target.JavaScriptLogger = fakeLogger;

            CallWithRequestBody(requestBody, "http://wwww.test.com/gibraltar/exception");


            fakeLogger.Received().LogException(Arg.Any<JavaScriptError>());
        }

        [Test]
        public void Should_pass_expected_object_to_logger()
        {
            var requestBody =
                 "{\"Category\":\"JavaScript\",\"Message\":\"Error: Test Error\",\"Url\":\"http://www.test.com/app.js\",\"StackTrace\":[\"createError/<@http://www.test.com/app.js:37:19\"],\"Cause\":\"\",\"Line\":37,\"Column\":18,\"Details\":\"\"}";

            var fakeLogger = Substitute.For<JavaScriptLogger>();
            _target.JavaScriptLogger = fakeLogger;

            CallWithRequestBody(requestBody, "http://wwww.test.com/gibraltar/exception");

            var expected = new JavaScriptError
            {
                Category = "JavaScript",
                Message = "Error: Test Error",
                Url = "http://www.test.com/app.js",
                StackTrace = new List<string> {"createError/<@http://www.test.com/app.js:37:19"},
                Cause = "",
                Line = 37,
                Column = 18,
                Details = ""
            }.ToExpectedObject();


            // ReSharper disable once SuspiciousTypeConversion.Global
            fakeLogger.Received().LogException(Arg.Is<JavaScriptError>(x => expected.Equals(x)));
        }
    }
}