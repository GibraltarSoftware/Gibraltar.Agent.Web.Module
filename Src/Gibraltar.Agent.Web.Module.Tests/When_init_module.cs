using System.Reflection;
using System.Web;
using NUnit.Framework;

namespace Gibraltar.Agent.Web.Module.Tests
{
    [TestFixture]
    public class When_init_module
    {
        [Test]
        public void Should_sucessfully_init_module()
        {
            var loggingModule = new Logging();
            var application = new HttpApplication();

            Assert.DoesNotThrow(() => loggingModule.Init(application));
        }

        [Test]
        public void Should_create_message_handler()
        {
            var loggingModule = new Logging();
            var application = new HttpApplication();

            loggingModule.Init(application);

            FieldInfo fieldInfo = typeof(Logging).GetField("_messageHandler",
                                     BindingFlags.NonPublic |
                                     BindingFlags.Instance);

            Assert.That(fieldInfo.GetValue(loggingModule), Is.Not.Null);

        }

    }
}
