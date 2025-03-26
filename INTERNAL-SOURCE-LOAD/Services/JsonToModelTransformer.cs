using INTERNAL_SOURCE_LOAD;
using System.Text.Json;

public class JsonToModelTransformer<T> : IJsonToModelTransformer<T>
{
    public T Transform(JsonElement jsonData)
    {
        if (jsonData.ValueKind == JsonValueKind.Undefined || jsonData.ValueKind == JsonValueKind.Null)
        {
            throw new ArgumentException("Invalid JSON payload: data cannot be null or undefined.", nameof(jsonData));
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
            };

            var result = JsonSerializer.Deserialize<T>(jsonData.GetRawText(), options);

            if (result == null)
            {
                throw new JsonException($"Failed to deserialize JSON into {typeof(T).Name}: resulting object is null.");
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to deserialize JSON into {typeof(T).Name}: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            // Convert other exceptions to JsonException for consistent error handling
            throw new JsonException($"Unexpected error deserializing JSON into {typeof(T).Name}: {ex.Message}", ex);
        }
    }
}
