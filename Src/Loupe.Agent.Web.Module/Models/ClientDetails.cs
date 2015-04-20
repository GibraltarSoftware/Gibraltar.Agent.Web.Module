using Newtonsoft.Json;

namespace Loupe.Agent.Web.Module.Models
{
    public class ClientDetails
    {
        public string Description { get; set; }

        public string Layout { get; set; }

        public string Manufacturer { get; set; }

        public string Name { get; set; }

        public string Prerelease { get; set; }

        public string Product { get; set; }

        [JsonProperty("ua")]
        public string UserAgentString { get; set; }

        public string Version { get; set; }

        public ClientOS OS { get; set; }

        public ClientDimensions Size { get; set; }

    }

    public class ClientOS
    {
        public int Architecture { get; set; }

        public string Family { get; set; }

        public string Version { get; set; }
    }

    public class ClientDimensions
    {
        public long Height { get; set; }
        public long Width { get; set; }
    }
}