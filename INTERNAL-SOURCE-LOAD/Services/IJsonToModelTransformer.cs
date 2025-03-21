using System.Text.Json;

namespace INTERNAL_SOURCE_LOAD;

public interface IJsonToModelTransformer<T>
{
    T Transform(JsonElement jsonData);
}
