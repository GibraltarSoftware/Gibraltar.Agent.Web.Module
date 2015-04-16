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
        protected HttpContextBase _httpContext;
        protected HttpRequestBase _httpRequest;
        protected HttpResponseBase _httpResponse;
        protected MessageHandler _target;
        protected MemoryStream _inputStream;

        [SetUp]
        public void SetUp()
        {
            _target = new MessageHandler();

            _httpContext = Substitute.For<HttpContextBase>();
            _httpRequest = Substitute.For<HttpRequestBase>();
            _httpResponse = Substitute.For<HttpResponseBase>();

            _inputStream = new MemoryStream();
            _httpRequest.InputStream.Returns(_inputStream);

            var fakeUser = Substitute.For<IPrincipal>();
            var fakeIdentity = Substitute.For<IIdentity>();
            fakeIdentity.Name.Returns("");
            fakeUser.Identity.Returns(fakeIdentity);



            _httpRequest.HttpMethod.Returns("POST");

            _httpContext.Request.Returns(_httpRequest);
            _httpContext.Response.Returns(_httpResponse);
            _httpContext.User.Returns(fakeUser);            
        }

        [TearDown]
        public void TearDown()
        {
            _inputStream.Dispose();
        }

        protected void CallWithRequestBody(string body, string url)
        {
            using (var writer = new StreamWriter(_inputStream))
            {
                writer.Write(body);
                writer.Flush();


                _httpRequest.Url.Returns(new Uri(url));
                _httpRequest.InputStream.Returns(_inputStream);

                _target.HandleRequest(_httpContext);
            }            
        }
    }
}