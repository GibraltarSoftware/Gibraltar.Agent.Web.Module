using System.Collections.Generic;

namespace Gibraltar.Agent.Web.Module.Models
{
    public class LogRequest
    {
        /// <summary>
        /// Stores session details e.g. client computer info
        /// </summary>
        public string Session { get; set; }

        /// <summary>
        /// List of <see cref="LogMessage"/> to be recorded
        /// </summary>
        public List<LogMessage> LogMessages { get; set; }
    }
}