using INTERNAL_SOURCE_LOAD;
using System.Text.Json;

public class JsonToModelTransformer<T> : IJsonToModelTransformer<T>
{
    public T Transform(JsonElement jsonData)
    {
        if (jsonData.ValueKind == JsonValueKind.Undefined || jsonData.ValueKind == JsonValueKind.Null)
        {
            throw new ArgumentException("Invalid JSON payload.");
        }

        return JsonSerializer.Deserialize<T>(jsonData.GetRawText(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        }) ?? throw new ArgumentException($"Failed to deserialize JSON into {typeof(T).Name}.");
    }
}
