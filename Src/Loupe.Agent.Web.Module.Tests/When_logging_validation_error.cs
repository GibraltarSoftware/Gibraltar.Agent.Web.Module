using System;
using System.Linq;
using System.Threading;
using System.Web;
using Gibraltar.Agent;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests
{
#if DEBUG
    [TestFixture]
    public class When_logging_validation_error:TestBase
    {
        private ManualResetEventSlim _resetEvent;
        private LogMessageAlertEventArgs _eventArgs;
        private HttpBrowserCapabilitiesBase _fakeBrowser;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Log.MessageAlert += Log_MessageAlert;
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Log.MessageAlert -= Log_MessageAlert;
        }

        [SetUp]
        public void SetUp()
        {
            _eventArgs = null;
            _resetEvent = new ManualResetEventSlim();


            _fakeBrowser = Substitute.For<HttpBrowserCapabilitiesBase>();
            _fakeBrowser.Browser.Returns("Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0");
            HttpRequest.Browser.Returns(_fakeBrowser);

            HttpRequest.ContentLength.Returns(10);
            HttpRequest.ContentType.Returns("application/json");
            HttpRequest.IsLocal.Returns(false);
            HttpRequest.IsSecureConnection.Returns(true);
            HttpRequest.UserHostAddress.Returns("216.58.211.4");
            HttpRequest.UserHostName.Returns("www.google.com");
        }

        [Test]
        public void Should_output_request_data_if_fails_validation()
        {
            HttpRequest.HttpMethod.Returns("PUT");

            SendRequest(null);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            var expectedDetails =
                "<Request><UserAgent>Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0</UserAgent><ContentType>application/json</ContentType><ContentLength>10</ContentLength><IsLocal>False</IsLocal><IsSecureConnection>True</IsSecureConnection><UserHostAddress>216.58.211.4</UserHostAddress><UserHostName>www.google.com</UserHostName></Request>";

            Assert.That(loggedMessage.Details, Is.EqualTo(expectedDetails));

        }

        [Test]
        public void Should_include_Request_body_if_unable_to_deserialize()
        {
            var invalidJson = "{ unclosedString: 'abc}";
            SendRequest(invalidJson);

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            var expectedDetails =
                "<Request><UserAgent>Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0</UserAgent><ContentType>application/json</ContentType><ContentLength>10</ContentLength><IsLocal>False</IsLocal><IsSecureConnection>True</IsSecureConnection><UserHostAddress>216.58.211.4</UserHostAddress><UserHostName>www.google.com</UserHostName><RequestBody>" +
                invalidJson + "</RequestBody></Request>";

            Assert.That(loggedMessage.Details, Is.EqualTo(expectedDetails));
        }


        void Log_MessageAlert(object sender, LogMessageAlertEventArgs e)
        {
            _eventArgs = e;
            e.MinimumDelay = new TimeSpan(0);
            _resetEvent.Set();
        }

        private void WaitForEvent()
        {
            _resetEvent.Wait(new TimeSpan(0, 0, 0, 5));
        }
    }
#endif
}