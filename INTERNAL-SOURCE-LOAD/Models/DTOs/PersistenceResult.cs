using System.Collections.Generic;

namespace INTERNAL_SOURCE_LOAD.Models.DTOs
{
    /// <summary>
    /// Represents the result of a data persistence operation.
    /// </summary>
    public class PersistenceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int SkippedDuplicates { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}