using System.Collections.Generic;
using System;
using INTERNAL_SOURCE_LOAD.Models.DTOs;

namespace INTERNAL_SOURCE_LOAD.Services
{
    public interface IDataPersistenceService
    {
        PersistenceResult PersistData(object model, string modelTypeName);
        bool IsDuplicateKeyError(Exception ex);
    }
}