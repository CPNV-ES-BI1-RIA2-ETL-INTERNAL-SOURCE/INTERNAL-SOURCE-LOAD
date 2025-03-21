using System.Text.Json;
using INTERNAL_SOURCE_LOAD.Models;

namespace INTERNAL_SOURCE_LOAD_TEST.TestData
{
    public abstract class TestDataFixture
    {
        protected static JsonElement ParseJson(string json)
        {
            return JsonDocument.Parse(json).RootElement;
        }

        protected static string SerializeToJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}