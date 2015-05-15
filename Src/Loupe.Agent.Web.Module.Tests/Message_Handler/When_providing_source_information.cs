using System.Collections.Generic;
using Loupe.Agent.Web.Module.Infrastructure;
using Loupe.Agent.Web.Module.Models;
using NUnit.Framework;


namespace Loupe.Agent.Web.Module.Tests.Message_Handler
{
    [TestFixture]
    public class When_providing_source_information
    {
        private JavaScriptSourceProvider _sourceProvider;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _sourceProvider = new JavaScriptSourceProvider();
        }

        [Test]
        public void Should_have_null_for_properties_where_no_MethodSourceInfo_or_Exception()
        {
            var message = new LogMessage();
            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.Null);
            Assert.That(target.FileName, Is.Null);
            Assert.That(target.LineNumber, Is.EqualTo(0));
        }

        [Test]
        public void Should_have_properties_set_from_MethodSourceInfo()
        {
            var message = new LogMessage
            {
                MethodSourceInfo = new MethodSourceInfo
                {
                    File = "app.js",
                    Line = 5
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.Null);
            Assert.That(target.FileName, Is.EqualTo("app.js"));
            Assert.That(target.LineNumber, Is.EqualTo(5));
        }

        [Test]
        public void Should_have_properties_set_from_firefox_stack()
        {
            var stack = new List<string>
            {
                "InnerItem/this.throwUnitializeError@http://localhost:3378/spec/helpers/Utils.js:92:9",
                "TestingStack/this.createError@http://localhost:3378/spec/helpers/Utils.js:74:9",
                "throwUninitializeError/<@http://localhost:3378/spec/When_logging_stack_trace.js:125:13"
            };

            var message = new LogMessage
            {
                Exception = new Exception
                {
                    Message = "uninitializedObject is undefined",
                    StackTrace = stack
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.EqualTo("InnerItem/this.throwUnitializeError"));
            Assert.That(target.FileName, Is.EqualTo("http://localhost:3378/spec/helpers/Utils.js"));
            Assert.That(target.LineNumber, Is.EqualTo(92));
        }

        [Test]
        public void Should_have_properties_set_from_chrome_stack()
        {
            var stack = new List<string>
            {
                "TypeError: Cannot read property 'doStuff' of undefined",
                "    at InnerItem.throwUnitializeError (http://localhost:3378/spec/helpers/Utils.js:92:28)",
                "    at TestingStack.createError (http://localhost:3378/spec/helpers/Utils.js:74:15)",
                "    at http://localhost:3378/spec/When_logging_stack_trace.js:125:20"
            };

            var message = new LogMessage
            {
                Exception = new Exception
                {
                    Message = "Uncaught TypeError: Cannot read property 'doStuff' of undefined",
                    StackTrace = stack
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.EqualTo("InnerItem.throwUnitializeError"));
            Assert.That(target.FileName, Is.EqualTo("http://localhost:3378/spec/helpers/Utils.js"));
            Assert.That(target.LineNumber, Is.EqualTo(92));
        }

        [Test]
        public void Should_have_properties_set_from_ie_stack()
        {
            var stack = new List<string>
            {
                "TypeError: Unable to get property 'doStuff' of undefined or null reference",
                "   at throwUnitializeError (http://localhost:3378/spec/helpers/Utils.js:92:9)",
                "   at createError (http://localhost:3378/spec/helpers/Utils.js:74:9)",
                "   at Anonymous function (http://localhost:3378/spec/When_logging_stack_trace.js:125:13)"
            };

            var message = new LogMessage
            {
                Exception = new Exception
                {
                    Message = "Uncaught TypeError: Cannot read property 'doStuff' of undefined",
                    StackTrace = stack
                }
            };

            var target = _sourceProvider.ProcessMessage(message);

            Assert.That(target.ClassName, Is.Null);
            Assert.That(target.MethodName, Is.EqualTo("throwUnitializeError"));
            Assert.That(target.FileName, Is.EqualTo("http://localhost:3378/spec/helpers/Utils.js"));
            Assert.That(target.LineNumber, Is.EqualTo(92));
        }
    }
}