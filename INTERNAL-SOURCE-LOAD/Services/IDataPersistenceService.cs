using System.Collections.Generic;

namespace INTERNAL_SOURCE_LOAD.Services
{
    public class PersistenceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int SkippedDuplicates { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public interface IDataPersistenceService
    {
        PersistenceResult PersistData(object model, string modelTypeName);
        bool IsDuplicateKeyError(Exception ex);
    }
}