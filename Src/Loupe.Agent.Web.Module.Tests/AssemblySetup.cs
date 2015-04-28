using Gibraltar.Agent;
using NUnit.Framework;

namespace Loupe.Agent.Web.Module.Tests
{
    [SetUpFixture]
    public class AssemblySetup
    {
        [SetUp]
        public void Setup()
        {
            Log.Initializing += Log_Initializing;

            Log.StartSession("Start Http Module Test Suite");
        }

        [TearDown]
        public void TearDown()
        {
            Log.EndSession("Test suite complete.");
        }

        void Log_Initializing(object sender, LogInitializingEventArgs e)
        {
            var publisherConfig = e.Configuration.Publisher;
            publisherConfig.ApplicationDescription = "Http Module Unit Tests";
            publisherConfig.ApplicationName = "Unit Tests";
            publisherConfig.ProductName = "Loupe.Agent.Web.Module";
            publisherConfig.EnvironmentName = "Development";
            
        }
    }
}