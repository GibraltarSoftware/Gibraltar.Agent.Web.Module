using System;
using System.Collections.Generic;
using ExpectedObjects;
using Gibraltar.Agent;
using Loupe.Agent.Web.Module.Models;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests
{
    [TestFixture]
    public class When_receives_request_to_log: TestBase
    {

        [Test]
        public void Should_return_200([Values("http://www.test.com/loupe/log",
                                              "http://www.test.com/Loupe/log",
                                              "http://www.test.com/loupe/Log",
                                              "http://www.test.com/Loupe/Log",
                                              "http://www.test.com/loupe/log/")] string url)
        {
            
            SendRequest("{Session:null, LogMessages:[]}");
 
            Assert.That(HttpResponse.StatusCode, Is.EqualTo(200));            
        }

        [Test]
        public void Should_call_logger()
        {
            var fakeLogger = Substitute.For<JavaScriptLogger>();

            Target.JavaScriptLogger = fakeLogger;

            SendRequest("{Session:null,LogMessages:[{severity: 8,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {}}]}");

            fakeLogger.Received().Log(Arg.Any<LogRequest>());
        }


        [Test]
        public void Should_pass_object_to_log()
        {
            var fakeLogger = Substitute.For<JavaScriptLogger>();
            Target.JavaScriptLogger = fakeLogger;

            SendRequest("{Session:null,LogMessages:[{severity: 8,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {}}]}");

            var expected = new LogRequest
            {
                Session = null,
                LogMessages = new List<LogMessage>
                {
                    new LogMessage {
                        Severity = LogMessageSeverity.Information,
                        Category = "Test",
                        Caption = "test log",
                        Description = "tests logs message",
                        Parameters = null,
                        Details = null,
                        Exception = new Error(),
                        MethodSourceInfo = new MethodSourceInfo(),
                        Sequence = 0,
                        TimeStamp = new DateTimeOffset()
                    }
                },
                User = FakeUser
            }.ToExpectedObject();

            // ReSharper disable once SuspiciousTypeConversion.Global
            fakeLogger.Received().Log(Arg.Is<LogRequest>(x => expected.Equals(x)));
        }

        [Test]
        public void Should_have_session_details()
        {
            const string requestBody = "{ session: { client: {description:'Firefox 37.0 32-bit on Windows 8.1 64-bit',layout:'Gecko',manufacturer:null,name:'Firefox',prerelease:null,product:null,ua:'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0',version:'37.0',os:{architecture:64,family:'Windows',version:'8.1'},size:{width:1102,height:873}}},LogMessages:[{severity: 8,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {}}]}";

            var fakeLogger = Substitute.For<JavaScriptLogger>();
            LogRequest actual = null;
            fakeLogger.Log(Arg.Do<LogRequest>(x => actual = x));            
            Target.JavaScriptLogger = fakeLogger;

            SendRequest(requestBody);


            var expected = new LogRequest
            {
                Session = new ClientSession
                {
                   Client = new ClientDetails
                   {
                       Description = "Firefox 37.0 32-bit on Windows 8.1 64-bit",
                       Layout = "Gecko",
                       Manufacturer = null,
                       Name = "Firefox",
                       Prerelease = null,
                       Product = null,
                       UserAgentString = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0",
                       Version = "37.0",
                       OS = new ClientOS
                       {
                           Architecture = 64,
                           Family = "Windows",
                           Version = "8.1"
                       },
                       Size = new ClientDimensions
                       {
                           Width = 1102,
                           Height = 873
                       }
                   }
                },
                LogMessages = new List<LogMessage>
                {
                    new LogMessage {
                        Severity = LogMessageSeverity.Information,
                        Category = "Test",
                        Caption = "test log",
                        Description = "tests logs message",
                        Parameters = null,
                        Details = null,
                        Exception = new Error(),
                        MethodSourceInfo = new MethodSourceInfo(),
                        Sequence = 0,
                        TimeStamp = new DateTimeOffset()
                    }
                },
                User = FakeUser
            }.ToExpectedObject();

            // ReSharper disable once SuspiciousTypeConversion.Global
            //fakeLogger.Received().Log(Arg.Is<LogRequest>(x => expected.Equals(x)));
            expected.ShouldEqual(actual);
        }

        [Test]
        public void Should_have_methodSourceInfo()
        {
            var fakeLogger = Substitute.For<JavaScriptLogger>();
            Target.JavaScriptLogger = fakeLogger;

            SendRequest("{Session:null,LogMessages:[{severity: 8,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: { file: 'app.js', line: 18, column: 37}}]}");

            var expected = new LogRequest
            {
                Session = null,
                LogMessages = new List<LogMessage>
                {
                    new LogMessage {
                        Severity = LogMessageSeverity.Information,
                        Category = "Test",
                        Caption = "test log",
                        Description = "tests logs message",
                        Parameters = null,
                        Details = null,
                        Exception = new Error(),
                        MethodSourceInfo = new MethodSourceInfo
                        {
                            File = "app.js",
                            Line = 18,
                            Column = 37

                        },
                        Sequence = 0,
                        TimeStamp = new DateTimeOffset()
                    }
                },
                User = FakeUser
            }.ToExpectedObject();

            // ReSharper disable once SuspiciousTypeConversion.Global
            fakeLogger.Received().Log(Arg.Is<LogRequest>(x => expected.Equals(x)));            
        }

        [Test]
        public void Should_have_expected_timestamp_and_sequence()
        {
            var currentDateTime = DateTime.Now;
            var timeStamp = new DateTimeOffset(currentDateTime, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));

            var jsonTimeStamp = timeStamp.ToString("yyyy-MM-ddTHH:mm:sszzz");

            var fakeLogger = Substitute.For<JavaScriptLogger>();
            LogRequest actual = null;
            fakeLogger.Log(Arg.Do<LogRequest>(x => actual = x));
            Target.JavaScriptLogger = fakeLogger;

            SendRequest("{Session:null,LogMessages:[{severity: 8,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {}, timeStamp: '" + jsonTimeStamp + "', sequence: 1}]}");

            var expected = new LogRequest
            {
                Session = null,
                LogMessages = new List<LogMessage>
                {
                    new LogMessage
                    {
                        Severity = LogMessageSeverity.Information,
                        Category = "Test",
                        Caption = "test log",
                        Description = "tests logs message",
                        Parameters = null,
                        Details = null,
                        Exception = new Error(),
                        MethodSourceInfo = new MethodSourceInfo(),
                        Sequence = 1,
                        TimeStamp = timeStamp
                    }
                },
                User = FakeUser
            }.ToExpectedObject()
             .Configure(ctx => ctx.PushStrategy<DateTimeOffSetComparisonStrategy>());

            // ReSharper disable once SuspiciousTypeConversion.Global
            //fakeLogger.Received().Log(Arg.Is<LogRequest>(x => expected.Equals(x)));
            expected.ShouldEqual(actual);
        }
    }
}