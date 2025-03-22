using INTERNAL_SOURCE_LOAD;
using INTERNAL_SOURCE_LOAD.Controllers;
using INTERNAL_SOURCE_LOAD.Models;
using INTERNAL_SOURCE_LOAD.Services;
using INTERNAL_SOURCE_LOAD_TEST.TestData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace INTERNAL_SOURCE_LOAD_TEST
{
  [TestFixture]
  public class LoadControllerTests
  {
    private Mock<IJsonTransformerService> _jsonTransformerServiceMock = null!;
    private Mock<IDataPersistenceService> _dataPersistenceServiceMock = null!;
    private Mock<IOptions<AppSettings>> _appSettingsMock = null!;
    private LoadController _controller = null!;
    private TrainStation _mockTrainStation = null!;

    [SetUp]
    public void Setup()
    {
      _jsonTransformerServiceMock = new Mock<IJsonTransformerService>();
      _dataPersistenceServiceMock = new Mock<IDataPersistenceService>();
      _appSettingsMock = new Mock<IOptions<AppSettings>>();

      _appSettingsMock.Setup(x => x.Value).Returns(new AppSettings
      {
        DefaultModel = typeof(TrainStation).AssemblyQualifiedName
      });

      // Create a mock train station object
      _mockTrainStation = TrainStationTestData.GetSimpleTrainStation().Station;

      // Setup the controller with the mocked services
      _controller = new LoadController(
        _jsonTransformerServiceMock.Object,
        _dataPersistenceServiceMock.Object,
        _appSettingsMock.Object
      );
    }

    [Test]
    public void Post_WhenDuplicateData_SkipsAndContinues()
    {
      // Arrange
      var jsonElement = JsonSerializer.Deserialize<JsonElement>(
        JsonSerializer.Serialize(TrainStationTestData.GetSimpleTrainStation())
      );

      _jsonTransformerServiceMock
        .Setup(x => x.TransformJsonToModel(It.IsAny<JsonElement>(), It.IsAny<string>()))
        .Returns(_mockTrainStation);

      var persistenceResult = new PersistenceResult
      {
        Success = true,
        SkippedDuplicates = 1,
        Message = "Data processed for table: TrainStation. Skipped 1 duplicate entries."
      };

      _dataPersistenceServiceMock
        .Setup(x => x.PersistData(It.IsAny<object>(), It.IsAny<string>()))
        .Returns(persistenceResult);

      // Act
      var result = _controller.Post(jsonElement);

      // Assert
      Assert.That(result, Is.InstanceOf<OkObjectResult>());
      var okResult = (OkObjectResult)result;
      Assert.That(okResult.Value.ToString(), Does.Contain("Skipped 1 duplicate entries"));
    }

    [Test]
    public void Post_WhenNonDuplicateData_ReturnsOk()
    {
      // Arrange
      var jsonElement = JsonSerializer.Deserialize<JsonElement>(
        JsonSerializer.Serialize(TrainStationTestData.GetSimpleTrainStation())
      );

      _jsonTransformerServiceMock
        .Setup(x => x.TransformJsonToModel(It.IsAny<JsonElement>(), It.IsAny<string>()))
        .Returns(_mockTrainStation);

      var persistenceResult = new PersistenceResult
      {
        Success = true,
        SkippedDuplicates = 0,
        Message = "Data processed for table: TrainStation"
      };

      _dataPersistenceServiceMock
        .Setup(x => x.PersistData(It.IsAny<object>(), It.IsAny<string>()))
        .Returns(persistenceResult);

      // Act
      var result = _controller.Post(jsonElement);

      // Assert
      Assert.That(result, Is.InstanceOf<OkObjectResult>());
      var okResult = (OkObjectResult)result;
      Assert.That(okResult.Value.ToString(), Does.Contain("Data processed for table"));
    }

    [Test]
    public void Post_WhenOtherDatabaseError_ReturnsInternalServerError()
    {
      // Arrange
      var jsonElement = JsonSerializer.Deserialize<JsonElement>(
        JsonSerializer.Serialize(TrainStationTestData.GetSimpleTrainStation())
      );

      _jsonTransformerServiceMock
        .Setup(x => x.TransformJsonToModel(It.IsAny<JsonElement>(), It.IsAny<string>()))
        .Returns(_mockTrainStation);

      var persistenceResult = new PersistenceResult
      {
        Success = false,
        Message = "Error processing data: Database connection failed"
      };

      _dataPersistenceServiceMock
        .Setup(x => x.PersistData(It.IsAny<object>(), It.IsAny<string>()))
        .Returns(persistenceResult);

      // Act
      var result = _controller.Post(jsonElement);

      // Assert
      Assert.That(result, Is.InstanceOf<ObjectResult>());
      var statusCodeResult = (ObjectResult)result;
      Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
      Assert.That(statusCodeResult.Value.ToString(), Does.Contain("Database connection failed"));
    }
  }
}
