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
        public void Should_pass_expected_exception_to_logger()
        {
            var fakeLogger = Substitute.For<JavaScriptLogger>();
            Target.JavaScriptLogger = fakeLogger;

            SendRequest("{Session:'',LogMessages:[{severity: 8,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {message:'TypeError: uninitializedObject is undefined',url:'http://www.test.com/app.js',stackTrace: [],cause:'', line: 37, column: 18},methodSourceInfo: {}}]}", LogUrl);

            var expected = new LogRequest
            {
                Session = "",
                LogMessages = new List<LogMessage>
                {
                    new LogMessage {
                        Severity = LogMessageSeverity.Information,
                        Category = "Test",
                        Caption = "test log",
                        Description = "tests logs message",
                        Parameters = null,
                        Details = null,
                        Exception = new Error
                        {
                            Message = "TypeError: uninitializedObject is undefined",
                            Url = "http://www.test.com/app.js",
                            StackTrace = new List<string>(),
                            Cause = "",
                            Line = 37,
                            Column = 18
                        },
                        MethodSourceInfo = new MethodSourceInfo(),
                        Sequence = 0,
                        TimeStamp = new DateTimeOffset()
                    }
                }
            }.ToExpectedObject();


            // ReSharper disable once SuspiciousTypeConversion.Global
            fakeLogger.Received().Log(Arg.Is<LogRequest>(x => expected.Equals(x)));
        }

        [Test]
        public void Should_pass_expected_object_with_stackTrace_to_logger()
        {

            var fakeLogger = Substitute.For<JavaScriptLogger>();
            Target.JavaScriptLogger = fakeLogger;

            SendRequest("{Session:'',LogMessages:[{severity: 8,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {message:'TypeError: uninitializedObject is undefined',url:'http://www.test.com/app.js',stackTrace: [\"InnerItem/this.throwUnitializeError\", \"TestingStack/this.createError\", \"throwUninitializeError\"],cause:'', line: 37, column: 18},methodSourceInfo: {}}]}", LogUrl);

            var expected = new LogRequest
            {
                Session = "",
                LogMessages = new List<LogMessage>
                {
                    new LogMessage {
                        Severity = LogMessageSeverity.Information,
                        Category = "Test",
                        Caption = "test log",
                        Description = "tests logs message",
                        Parameters = null,
                        Details = null,
                        Exception = new Error
                        {
                            Message = "TypeError: uninitializedObject is undefined",
                            Url = "http://www.test.com/app.js",
                            StackTrace = new List<string> { "InnerItem/this.throwUnitializeError", "TestingStack/this.createError", "throwUninitializeError" },
                            Cause = "",
                            Line = 37,
                            Column = 18
                        },
                        MethodSourceInfo = new MethodSourceInfo(),
                        Sequence = 0,
                        TimeStamp = new DateTimeOffset()
                    }
                }
            }.ToExpectedObject();


            // ReSharper disable once SuspiciousTypeConversion.Global
            fakeLogger.Received().Log(Arg.Is<LogRequest>(x => expected.Equals(x)));
        }

        [Test]
        public void Should_have_session_details()
        {
            var requestBody =
                "{Session:\"{\\\"Client\\\":{\\\"description\\\":\\\"Firefox 37.0 32-bit on Windows 8.1 64-bit\\\",\\\"layout\\\":\\\"Gecko\\\",\\\"manufacturer\\\":null,\\\"name\\\":\\\"Firefox\\\",\\\"prerelease\\\":null,\\\"product\\\":null,\\\"ua\\\":\\\"Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0\\\",\\\"version\\\":\\\"37.0\\\",\\\"os\\\":{\\\"architecture\\\":64,\\\"family\\\":\\\"Windows\\\",\\\"version\\\":\\\"8.1\\\"},\\\"size\\\":{\\\"width\\\":1102,\\\"height\\\":873}}\",LogMessages:[{severity: 8,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {message:'TypeError: uninitializedObject is undefined',url:'http://www.test.com/app.js',stackTrace: [\"InnerItem/this.throwUnitializeError\", \"TestingStack/this.createError\", \"throwUninitializeError\"],cause:'', line: 37, column: 18},methodSourceInfo: {}}]}";

            var fakeLogger = Substitute.For<JavaScriptLogger>();
            Target.JavaScriptLogger = fakeLogger;

            SendRequest(requestBody, LogUrl);


            var expected = new LogRequest
            {
                Session = "{\"Client\":{\"description\":\"Firefox 37.0 32-bit on Windows 8.1 64-bit\",\"layout\":\"Gecko\",\"manufacturer\":null,\"name\":\"Firefox\",\"prerelease\":null,\"product\":null,\"ua\":\"Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0\",\"version\":\"37.0\",\"os\":{\"architecture\":64,\"family\":\"Windows\",\"version\":\"8.1\"},\"size\":{\"width\":1102,\"height\":873}}" ,
                LogMessages = new List<LogMessage>
                {
                    new LogMessage {
                        Severity = LogMessageSeverity.Information,
                        Category = "Test",
                        Caption = "test log",
                        Description = "tests logs message",
                        Parameters = null,
                        Details = null,
                        Exception = new Error
                        {
                            Message = "TypeError: uninitializedObject is undefined",
                            Url = "http://www.test.com/app.js",
                            StackTrace = new List<string> { "InnerItem/this.throwUnitializeError", "TestingStack/this.createError", "throwUninitializeError" },
                            Cause = "",
                            Line = 37,
                            Column = 18
                        },
                        MethodSourceInfo = new MethodSourceInfo(),
                        Sequence = 0,
                        TimeStamp = new DateTimeOffset()
                    }
                }
            }.ToExpectedObject();

            // ReSharper disable once SuspiciousTypeConversion.Global
            fakeLogger.Received().Log(Arg.Is<LogRequest>(x => expected.Equals(x)));
        }
    }
}