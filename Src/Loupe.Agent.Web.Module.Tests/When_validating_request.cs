using System;
using System.IO;
using NSubstitute;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests
{
    [TestFixture]
    public class When_validating_request: TestBase
    {
        [Test]
        public void Should_only_handle_POST([Values("GET", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS")] string method)
        {
            HttpRequest.HttpMethod.Returns(method);

            SendRequest(null);

            Assert.That(HttpResponse.StatusCode, Is.EqualTo(0));
        }

        [Test]
        public void Should_return_400_if_no_content_in_stream()
        {
            SendRequest(null);

            Assert.That(HttpResponse.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void Should_return_400_if_content_exceeds_2k()
        {
            using (var stream = new MemoryStream())
            using(var writer = new StreamWriter(stream))
            {
                do
                {
                    writer.Write('X');
                    writer.Flush();
                } while (stream.Length < 2049);

                HttpRequest.Url.Returns(new Uri(LogUrl));
                HttpRequest.InputStream.Returns(stream);

                Target.HandleRequest(HttpContext);
            }

            Assert.That(HttpResponse.StatusCode, Is.EqualTo(400));
        }
    }
}