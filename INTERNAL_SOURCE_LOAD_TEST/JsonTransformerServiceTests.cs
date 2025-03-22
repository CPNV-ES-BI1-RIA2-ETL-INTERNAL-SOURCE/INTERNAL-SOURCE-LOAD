using INTERNAL_SOURCE_LOAD;
using INTERNAL_SOURCE_LOAD.Models;
using INTERNAL_SOURCE_LOAD.Services;
using INTERNAL_SOURCE_LOAD_TEST.TestData;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace INTERNAL_SOURCE_LOAD_TEST
{
    [TestFixture]
    public class JsonTransformerServiceTests
    {
        private JsonTransformerService _service = null!;
        private Mock<IServiceProvider> _serviceProviderMock = null!;
        private Mock<IJsonToModelTransformer<TrainStation>> _transformerMock = null!;
        private TrainStation _mockTrainStation = null!;

        [SetUp]
        public void Setup()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _transformerMock = new Mock<IJsonToModelTransformer<TrainStation>>();
            _mockTrainStation = TrainStationTestData.GetSimpleTrainStation().Station;

            // Setup the transformer mock
            _transformerMock
                .Setup(x => x.Transform(It.IsAny<JsonElement>()))
                .Returns(_mockTrainStation);

            // Setup the service provider to return our transformer mock
            _serviceProviderMock
                .Setup(x => x.GetService(It.Is<Type>(t => t.Name.Contains("IJsonToModelTransformer"))))
                .Returns(_transformerMock.Object);

            _service = new JsonTransformerService(_serviceProviderMock.Object);
        }

        [Test]
        public void TransformJsonToModel_ValidData_ReturnsTransformedModel()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(
                JsonSerializer.Serialize(TrainStationTestData.GetSimpleTrainStation())
            );
            string modelTypeName = typeof(TrainStation).AssemblyQualifiedName!;

            // Act
            var result = _service.TransformJsonToModel(jsonElement, modelTypeName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<TrainStation>());
            var trainStation = (TrainStation)result;
            Assert.That(trainStation.Name, Is.EqualTo(_mockTrainStation.Name));
        }

        [Test]
        public void TransformJsonToModel_InvalidModelType_ThrowsArgumentException()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(
                JsonSerializer.Serialize(TrainStationTestData.GetSimpleTrainStation())
            );
            string invalidTypeName = "INTERNAL_SOURCE_LOAD.Models.NonExistentModel, INTERNAL-SOURCE-LOAD";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _service.TransformJsonToModel(jsonElement, invalidTypeName));

            Assert.That(exception.Message, Does.Contain("not found"));
        }

        [Test]
        public void TransformJsonToModel_NoTransformerFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(
                JsonSerializer.Serialize(TrainStationTestData.GetSimpleTrainStation())
            );
            string modelTypeName = typeof(TrainStation).AssemblyQualifiedName!;

            // Setup service provider to return null for transformer
            _serviceProviderMock
                .Setup(x => x.GetService(It.Is<Type>(t => t.Name.Contains("IJsonToModelTransformer"))))
                .Returns(null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _service.TransformJsonToModel(jsonElement, modelTypeName));

            Assert.That(exception.Message, Does.Contain("No transformer found"));
        }

        [Test]
        public void TransformJsonToModel_TransformerReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(
                JsonSerializer.Serialize(TrainStationTestData.GetSimpleTrainStation())
            );
            string modelTypeName = typeof(TrainStation).AssemblyQualifiedName!;

            // Setup transformer to return null
            _transformerMock
                .Setup(x => x.Transform(It.IsAny<JsonElement>()))
                .Returns((TrainStation)null!);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _service.TransformJsonToModel(jsonElement, modelTypeName));

            Assert.That(exception.Message, Does.Contain("null model"));
        }
    }
}