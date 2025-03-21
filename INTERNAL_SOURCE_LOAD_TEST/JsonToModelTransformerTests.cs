using INTERNAL_SOURCE_LOAD;
using INTERNAL_SOURCE_LOAD_TEST.TestData;
using System.Text.Json;

namespace INTERNAL_SOURCE_LOAD_TEST
{
    [TestFixture]
    public class JsonToModelTransformerTests
    {
        private IJsonToModelTransformer<JsonTestData.TestModel> _transformer = null!;
        private Mock<IJsonToModelTransformer<JsonTestData.TestModel>> _transformerMock = null!;

        [SetUp]
        public void Setup()
        {
            // For tests where we want to test the actual implementation
            _transformer = new JsonToModelTransformer<JsonTestData.TestModel>();

            // For tests where we want to mock the behavior
            _transformerMock = new Mock<IJsonToModelTransformer<JsonTestData.TestModel>>();
        }

        [Test]
        public void Transform_ValidJson_ReturnsDeserializedObject()
        {
            // Arrange
            var validJson = JsonTestData.GetValidJson();
            var expectedModel = JsonTestData.GetExpectedModel();

            // Act
            var result = _transformer.Transform(validJson);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(expectedModel.Name));
            Assert.That(result.Age, Is.EqualTo(expectedModel.Age));
        }

        [Test]
        public void Transform_InvalidJson_ThrowsArgumentException()
        {
            // Arrange
            var invalidJson = JsonTestData.GetInvalidJson();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _transformer.Transform(invalidJson));
            Assert.That(ex.Message, Is.EqualTo("Invalid JSON payload."));
        }

        [Test]
        public void Transform_EmptyJson_ThrowsJsonException()
        {
            // Arrange
            var emptyJson = JsonDocument.Parse("{}").RootElement;

            // Act & Assert
            Assert.Throws<System.Text.Json.JsonException>(() => _transformer.Transform(emptyJson));
        }

        [Test]
        public void Transform_PartialJson_WithRequiredProperty_DeserializesCorrectly()
        {
            // Arrange
            var partialJson = JsonDocument.Parse("{\"name\":\"PartialName\"}").RootElement;

            // Act
            var result = _transformer.Transform(partialJson);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("PartialName"));
            Assert.That(result.Age, Is.EqualTo(0)); // Default value since not provided
        }

        [Test]
        public void Transform_CaseInsensitiveJson_DeserializesCorrectly()
        {
            // Arrange
            var caseInsensitiveJson = JsonDocument.Parse("{\"NAME\":\"TestName\",\"AGE\":35}").RootElement;

            // Act
            var result = _transformer.Transform(caseInsensitiveJson);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("TestName"));
            Assert.That(result.Age, Is.EqualTo(35));
        }

        [Test]
        public void MockedTransform_ReturnsExpectedResult()
        {
            // Arrange
            var jsonElement = JsonDocument.Parse("{}").RootElement;
            var expectedModel = JsonTestData.GetExpectedModel();

            _transformerMock
                .Setup(x => x.Transform(It.IsAny<JsonElement>()))
                .Returns(expectedModel);

            // Act
            var result = _transformerMock.Object.Transform(jsonElement);

            // Assert
            Assert.That(result, Is.SameAs(expectedModel));
            _transformerMock.Verify(x => x.Transform(It.IsAny<JsonElement>()), Times.Once);
        }
    }
}
