using System.Collections.Generic;

namespace Loupe.Agent.Web.Module.Models
{
    public class LogRequest
    {
        /// <summary>
        /// Stores session details e.g. client computer info
        /// </summary>
        public ClientSession Session { get; set; }

        /// <summary>
        /// List of <see cref="LogMessage"/> to be recorded
        /// </summary>
        public List<LogMessage> LogMessages { get; set; }
    }
}