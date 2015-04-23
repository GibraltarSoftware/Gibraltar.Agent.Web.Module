using System;
using System.IO;
using System.Security.Principal;
using System.Web;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests
{
    public class TestBase
    {
        protected const string LogUrl = "http://www.test.com/loupe/log";

        protected HttpContextBase HttpContext;
        protected HttpRequestBase HttpRequest;
        protected HttpResponseBase HttpResponse;
        protected MessageHandler Target;
        protected MemoryStream InputStream;
        protected IPrincipal FakeUser;
        protected IIdentity FakeIdentity;
        protected string DefaultTestSessionId;

        [SetUp]
        public void BaseSetUp()
        {
            Target = new MessageHandler();

            HttpContext = Substitute.For<HttpContextBase>();
            HttpRequest = Substitute.For<HttpRequestBase>();
            HttpResponse = Substitute.For<HttpResponseBase>();

            InputStream = new MemoryStream();
            HttpRequest.InputStream.Returns(InputStream);
            

            FakeUser = Substitute.For<IPrincipal>();
            FakeIdentity = Substitute.For<IIdentity>();
            FakeIdentity.Name.Returns("");
            FakeUser.Identity.Returns(FakeIdentity);

            HttpRequest.HttpMethod.Returns("POST");
            HttpRequest.Cookies.Returns(new HttpCookieCollection());
            HttpRequest.Cookies.Add(new HttpCookie("Loupe", Guid.Empty.ToString()));
            DefaultTestSessionId = Guid.Empty.ToString();

            HttpContext.Request.Returns(HttpRequest);
            HttpContext.Response.Returns(HttpResponse);
            HttpContext.User.Returns(FakeUser);            
        }

        [TearDown]
        public void TearDown()
        {
            InputStream.Dispose();
        }

        protected void SendRequest(string body)
        {
            using (var writer = new StreamWriter(InputStream))
            {
                writer.Write(body);
                writer.Flush();


                HttpRequest.Url.Returns(new Uri(LogUrl));
                HttpRequest.InputStream.Returns(InputStream);

                Target.HandleRequest(HttpContext);
            }            
        }
    }
}