using System.Text.Json;

namespace INTERNAL_SOURCE_LOAD.Services
{
    public interface IJsonTransformerService
    {
        object TransformJsonToModel(JsonElement jsonData, string modelTypeName);
        Type ResolveModelType(string modelTypeName);
    }
}