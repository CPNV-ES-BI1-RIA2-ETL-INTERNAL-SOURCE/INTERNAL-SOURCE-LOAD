using System.Text.Json;

namespace INTERNAL_SOURCE_LOAD_TEST.TestData
{
    public class JsonTestData : TestDataFixture
    {
        public class TestModel
        {
            public required string Name { get; set; }
            public int Age { get; set; }
        }

        public static JsonElement GetValidJson()
        {
            return ParseJson("""{"name":"TestName","age":30}""");
        }

        public static JsonElement GetInvalidJson()
        {
            return ParseJson("null");
        }

        public static TestModel GetExpectedModel()
        {
            return new TestModel
            {
                Name = "TestName",
                Age = 30
            };
        }
    }
}