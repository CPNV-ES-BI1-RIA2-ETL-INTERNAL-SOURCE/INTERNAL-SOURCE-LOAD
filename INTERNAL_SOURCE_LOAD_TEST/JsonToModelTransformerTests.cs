using System.Text.Json;
using System.Xml.Linq;
using Aspose.Cells;
using INTERNAL_SOURCE_LOAD;
using INTERNAL_SOURCE_LOAD.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework.Legacy;


namespace INTERNAL_SOURCE_LOAD_TEST
{
    [TestFixture]
    public class JsonToModelTransformerTests
    {
        private JsonToModelTransformer<TestModel> _transformer;

        [SetUp]
        public void Setup()
        {
            _transformer = new JsonToModelTransformer<TestModel>();
        }

        [Test]
        public void Transform_ValidJson_ReturnsDeserializedObject()
        {
            // Arrange
            var validJson = JsonDocument.Parse("""{"name":"TestName","age":30}""").RootElement;

            // Act
            var result = _transformer.Transform(validJson);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("TestName"));
            Assert.That(result.Age, Is.EqualTo(30));
        }

        [Test]
        public void Transform_InvalidJson_ThrowsArgumentException()
        {
            // Arrange
            var invalidJson = JsonDocument.Parse("null").RootElement;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _transformer.Transform(invalidJson));
            Assert.That(ex.Message, Is.EqualTo("Invalid JSON payload."));
        }

        public class TestModel
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
