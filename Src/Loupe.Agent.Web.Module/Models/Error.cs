using System.Collections.Generic;

namespace Loupe.Agent.Web.Module.Models
{
    public class Error
    {
        /// <summary>
        /// The message associated with the error
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The URL upon which the error occurred
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The stack trace
        /// </summary>
        public List<string> StackTrace { get; set; }

        /// <summary>
        /// Optional. The cause of the error
        /// </summary>
        public string Cause { get; set; }

        /// <summary>
        /// Optional. The line number upon which the error occurred
        /// </summary>
        public int? Line { get; set; }

        /// <summary>
        /// Optional. The column number upon which the error occurred
        /// </summary>
        public int? Column { get; set; }


        /// <summary>
        /// Indicates if the class is in fact empty i.e created but with no values
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Message == null && Url == null && Cause == null && !Line.HasValue && !Column.HasValue;
        }
    }
}