namespace Loupe.Agent.Web.Module.Models
{
    public class MethodSourceInfo
    {
        /// <summary>
        /// File that the error occured in
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Optional. The line number upon which the error occurred
        /// </summary>
        public int? Line { get; set; }

        /// <summary>
        /// Optional. The column number upon which the error occurred
        /// </summary>
        public int? Column { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(File) && !Line.HasValue && !Column.HasValue;
        }
    }
}