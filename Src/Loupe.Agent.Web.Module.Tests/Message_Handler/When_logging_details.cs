using System;
using System.Linq;
using System.Threading;
using System.Web;
using Gibraltar.Agent;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests.Message_Handler
{
    /// <summary>
    /// All messages sent in these tests have their severity set to Warning to ensure that
    /// the Log.MessageAlert event is rasied simply so we can interrogate the details we
    /// have logged to ensure they are created correctly
    /// </summary>
    [TestFixture]
    public class When_logging_details:TestBase
    {
        private ManualResetEventSlim _resetEvent;
        private LogMessageAlertEventArgs _eventArgs;

        private const string ExpectedMethodSourceInfo =
            "<MethodSourceInfo><File>app.js</File><Line>3</Line><Column>5</Column></MethodSourceInfo>";

        private const string ExpectedClientDetails =
            "<ClientDetails><Description>Firefox 37.0 32-bit on Windows 8.1 64-bit</Description><Layout>Gecko</Layout><Name>Firefox</Name><UserAgentString>Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0</UserAgentString><Version>37.0</Version><OS><Architecture>64</Architecture><Family>Windows</Family><Version>8.1</Version></OS><Size><Height>873</Height><Width>1102</Width></Size></ClientDetails>";

        private const string ExpectedUserSuppliedJson = "<UserSupplied><numericValue>1</numericValue><stringValue>text value</stringValue><objectValue><childNumber>3</childNumber><childText>child text</childText></objectValue></UserSupplied>";

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
        }

        [Test]
        public void Should_log_expected_timestamp_and_sequence()
        {
            var currentDateTime = DateTime.Now;
            var timeStamp = new DateTimeOffset(currentDateTime, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));

            var jsonTimeStamp = timeStamp.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

            SendRequest("{ session: { client: {description:'Firefox 37.0 32-bit on Windows 8.1 64-bit',layout:'Gecko',manufacturer:null,name:'Firefox',prerelease:null,product:null,ua:'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0',version:'37.0',os:{architecture:64,family:'Windows',version:'8.1'},size:{width:1102,height:873}}},LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {}, timeStamp: '" + jsonTimeStamp + "', sequence: 1}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Is.StringContaining("<TimeStamp>" + timeStamp + "</TimeStamp><Sequence>1</Sequence>"));
        }

        [Test]
        public void Should_not_log_timestamp_if_not_provided()
        {
            SendRequest("{Session:null,LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: '',exception: {},methodSourceInfo: {}}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Is.Not.StringContaining("<TimeStamp>01/01/0001 00:00:00 +00:00</TimeStamp>"));            
        }

        [Test]
        public void Should_log_method_source_info_details()
        {

            SendRequest("{Session:null,LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Is.StringContaining(ExpectedMethodSourceInfo));
        }

        [Test]
        public void Should_log_client_details()
        {
            SendRequest("{session: { client: {description:'Firefox 37.0 32-bit on Windows 8.1 64-bit',layout:'Gecko',manufacturer:null,name:'Firefox',prerelease:null,product:null,ua:'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0',version:'37.0',os:{architecture:64,family:'Windows',version:'8.1'},size:{width:1102,height:873}}},logMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Is.StringContaining(ExpectedClientDetails));
        }

        [Test]
        public void Should_output_user_supplied_JSON_as_xml()
        {
            SendRequest("{Session:null,LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: \"{ numericValue: 1, stringValue: 'text value', objectValue: {childNumber: 3, childText: 'child text'}}\",exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Is.StringContaining(ExpectedUserSuppliedJson));

        }

        [Test]
        public void Should_output_user_supplied_details_as_string()
        {
            SendRequest("{Session:null,LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: 'this is user supplied details',exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Is.StringContaining("<UserSupplied>this is user supplied details</UserSupplied>"));            
        }

        [Test]
        public void Should_output_session_id_in_details_block_even_if_no_session_details()
        {
            SendRequest("{Session:null,LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: null,methodSourceInfo: null}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Is.StringContaining("<SessionId>" + Guid.Empty + "</SessionId>"));            
            
        }

        [Test]
        public void Should_output_session_id_in_details_block_when_session_details_exist()
        {
            HttpRequest.Cookies.Clear();

            var sessionId = Guid.NewGuid().ToString();
            var loupeCookie = new HttpCookie(LoupeCookieName, sessionId);

            HttpRequest.Cookies.Add(loupeCookie);

            SendRequest("{Session:{ client: {description:'Firefox 37.0 32-bit on Windows 8.1 64-bit',layout:'Gecko',manufacturer:null,name:'Firefox',prerelease:null,product:null,ua:'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0',version:'37.0',os:{architecture:64,family:'Windows',version:'8.1'},size:{width:1102,height:873}}},LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: null,methodSourceInfo: null}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Is.StringContaining("<SessionId>" + sessionId + "</SessionId>"));

        }

        [Test]
        public void Should_output_session_id_from_request_when_exists_and_no_cookie()
        {
            HttpRequest.Cookies.Clear();

            SendRequest("{Session:{sessionId: 'session-123'},LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: null,methodSourceInfo: null}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Is.StringContaining("<SessionId>session-123</SessionId>"));            
            
        }

        [Test]
        public void Should_output_session_id_from_request_when_exists_even_if_cookie_present()
        {
            SendRequest("{Session:{sessionId: 'session-123'},LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: null,methodSourceInfo: null}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            Assert.That(loggedMessage.Details, Is.StringContaining("<SessionId>session-123</SessionId>"));

        }

        [Test]
        public void Should_not_output_session_id_in_details_if_no_cookie_or_id_in_request()
        {
            HttpRequest.Cookies.Clear();

            SendRequest("{Session:{ client: {description:'Firefox 37.0 32-bit on Windows 8.1 64-bit',layout:'Gecko',manufacturer:null,name:'Firefox',prerelease:null,product:null,ua:'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0',version:'37.0',os:{architecture:64,family:'Windows',version:'8.1'},size:{width:1102,height:873}}},LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: null,exception: null,methodSourceInfo: null}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);
            
            Assert.That(loggedMessage.Details, Is.Not.StringContaining("<SessionId>"));
        }

        [Test]
        public void Should_output_expected_details_block()
        {
            HttpRequest.Cookies.Clear();

            var sessionId = Guid.NewGuid().ToString();
            var loupeCookie = new HttpCookie(LoupeCookieName, sessionId);

            HttpRequest.Cookies.Add(loupeCookie);

            var currentDateTime = DateTime.Now;
            var timeStamp = new DateTimeOffset(currentDateTime, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));

            var jsonTimeStamp = timeStamp.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

            SendRequest("{ session: { client: {description:'Firefox 37.0 32-bit on Windows 8.1 64-bit',layout:'Gecko',manufacturer:null,name:'Firefox',prerelease:null,product:null,ua:'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0',version:'37.0',os:{architecture:64,family:'Windows',version:'8.1'},size:{width:1102,height:873}}},LogMessages:[{severity: 4,category: 'Test',caption: 'test log',description: 'tests logs message',paramters: null,details: \"{ numericValue: 1, stringValue: 'text value', objectValue: {childNumber: 3, childText: 'child text'}}\",exception: {},methodSourceInfo: {file:'app.js', line: 3, column: 5}, timeStamp: '" + jsonTimeStamp + "', sequence: 1}]}");

            WaitForEvent();

            var loggedMessage = _eventArgs.Messages.FirstOrDefault();

            Assert.That(loggedMessage, Is.Not.Null);

            var expectedDetailsBlock = "<Details><SessionId>" + sessionId + "</SessionId><TimeStamp>" + timeStamp + "</TimeStamp><Sequence>1</Sequence>" +
                                       ExpectedClientDetails + ExpectedMethodSourceInfo + ExpectedUserSuppliedJson + "</Details>";

            Assert.That(loggedMessage.Details, Is.EqualTo(expectedDetailsBlock));
            
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
}