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
            SendRequest("{\"Category\":\"JavaScript\"}",url);

            Assert.That(HttpResponse.StatusCode, Is.EqualTo(200));
        }


        [Test]
        public void Should_call_logger()
        {
            var requestBody =
                "{\"Category\":\"JavaScript\",\"Message\":\"Error: Test Error\",\"Url\":\"http://www.test.com/app.js\",\"StackTrace\":[\"createError/<@http://www.test.com/app.js:37:19\"],\"Cause\":\"\",\"Line\":37,\"Column\":18,\"Details\":\"\"}";

            var fakeLogger = Substitute.For<JavaScriptLogger>();

            Target.JavaScriptLogger = fakeLogger;

            SendRequest(requestBody, ExceptionUrl);


            fakeLogger.Received().LogException(Arg.Any<JavaScriptError>());
        }

        [Test]
        public void Should_pass_expected_object_to_logger()
        {
            var requestBody =
                 "{\"Category\":\"JavaScript\",\"Message\":\"Error: Test Error\",\"Url\":\"http://www.test.com/app.js\",\"StackTrace\":[\"createError/<@http://www.test.com/app.js:37:19\"],\"Cause\":\"\",\"Line\":37,\"Column\":18,\"Details\":\"\"}";

            var fakeLogger = Substitute.For<JavaScriptLogger>();
            Target.JavaScriptLogger = fakeLogger;

            SendRequest(requestBody, ExceptionUrl);

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

        [Test]
        public void Should_pass_expected_object_with_details_to_logger()
        {

            var requestBody =
                "{\"Category\":\"JavaScript\",\"Message\":\"Error: Test Error\",\"Url\":\"http://www.test.com/app.js\",\"StackTrace\":[\"createError/<@http://www.test.com/app.js:37:19\"],\"Cause\":\"\",\"Line\":37,\"Column\":18,\"Details\":\"{\\\"Client\\\":{\\\"description\\\":\\\"Firefox 37.0 32-bit on Windows 8.1 64-bit\\\",\\\"layout\\\":\\\"Gecko\\\",\\\"manufacturer\\\":null,\\\"name\\\":\\\"Firefox\\\",\\\"prerelease\\\":null,\\\"product\\\":null,\\\"ua\\\":\\\"Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0\\\",\\\"version\\\":\\\"37.0\\\",\\\"os\\\":{\\\"architecture\\\":64,\\\"family\\\":\\\"Windows\\\",\\\"version\\\":\\\"8.1\\\"},\\\"size\\\":{\\\"width\\\":1102,\\\"height\\\":873}}}\"}";

            var fakeLogger = Substitute.For<JavaScriptLogger>();

            JavaScriptError actual = null;
            fakeLogger.LogException(Arg.Do<JavaScriptError>(x => actual = x));

            Target.JavaScriptLogger = fakeLogger;

            SendRequest(requestBody, ExceptionUrl);

            var expected = new JavaScriptError
            {
                Category = "JavaScript",
                Message = "Error: Test Error",
                Url = "http://www.test.com/app.js",
                StackTrace = new List<string> { "createError/<@http://www.test.com/app.js:37:19" },
                Cause = "",
                Line = 37,
                Column = 18,
                Details = "{\"Client\":{\"description\":\"Firefox 37.0 32-bit on Windows 8.1 64-bit\",\"layout\":\"Gecko\",\"manufacturer\":null,\"name\":\"Firefox\",\"prerelease\":null,\"product\":null,\"ua\":\"Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0\",\"version\":\"37.0\",\"os\":{\"architecture\":64,\"family\":\"Windows\",\"version\":\"8.1\"},\"size\":{\"width\":1102,\"height\":873}}}" // "{\\\"Client\\\":{\\\"description\\\":\\\"Firefox 37.0 32-bit on Windows 8.1 64-bit\\\",\\\"layout\\\":\\\"Gecko\\\",\\\"manufacturer\\\":null,\\\"name\\\":\\\"Firefox\\\",\\\"prerelease\\\":null,\\\"product\\\":null,\\\"ua\\\":\\\"Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0\\\",\\\"version\\\":\\\"37.0\\\",\\\"os\\\":{\\\"architecture\\\":64,\\\"family\\\":\\\"Windows\\\",\\\"version\\\":\\\"8.1\\\"},\\\"size\\\":{\\\"width\\\":1102,\\\"height\\\":873}}}"
            }.ToExpectedObject();


            // ReSharper disable once SuspiciousTypeConversion.Global
            fakeLogger.Received().LogException(Arg.Is<JavaScriptError>(x => expected.Equals(x)));
        }
    }
}