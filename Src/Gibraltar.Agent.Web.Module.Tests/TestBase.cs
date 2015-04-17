using System;
using System.IO;
using System.Security.Principal;
using System.Web;
using NSubstitute;
using NUnit.Framework;

namespace Gibraltar.Agent.Web.Module.Tests
{
    public class TestBase
    {
        protected const string LogUrl = "http://www.test.com/gibraltar/log";
        protected const string ExceptionUrl = "http://www.test.com/gibraltar/exception";

        protected HttpContextBase HttpContext;
        protected HttpRequestBase HttpRequest;
        protected HttpResponseBase HttpResponse;
        protected MessageHandler Target;
        protected MemoryStream InputStream;


        [SetUp]
        public void SetUp()
        {
            Target = new MessageHandler();

            HttpContext = Substitute.For<HttpContextBase>();
            HttpRequest = Substitute.For<HttpRequestBase>();
            HttpResponse = Substitute.For<HttpResponseBase>();

            InputStream = new MemoryStream();
            HttpRequest.InputStream.Returns(InputStream);

            var fakeUser = Substitute.For<IPrincipal>();
            var fakeIdentity = Substitute.For<IIdentity>();
            fakeIdentity.Name.Returns("");
            fakeUser.Identity.Returns(fakeIdentity);

            HttpRequest.HttpMethod.Returns("POST");

            HttpContext.Request.Returns(HttpRequest);
            HttpContext.Response.Returns(HttpResponse);
            HttpContext.User.Returns(fakeUser);            
        }

        [TearDown]
        public void TearDown()
        {
            InputStream.Dispose();
        }

        protected void SendRequest(string body, string url)
        {
            using (var writer = new StreamWriter(InputStream))
            {
                writer.Write(body);
                writer.Flush();


                HttpRequest.Url.Returns(new Uri(url));
                HttpRequest.InputStream.Returns(InputStream);

                Target.HandleRequest(HttpContext);
            }            
        }
    }
}