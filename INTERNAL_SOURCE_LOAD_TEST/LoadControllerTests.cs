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
    private Mock<IServiceProvider> _serviceProviderMock = null!;
    private Mock<IOptions<AppSettings>> _appSettingsMock = null!;
    private Mock<IDatabaseExecutor> _sqlExecutorMock = null!;
    private Mock<IJsonToModelTransformer<TrainStation>> _transformerMock = null!;
    private LoadController _controller = null!;
    private TrainStation _mockTrainStation = null!;
    private List<(string, object)> _mockSqlQueries = null!;

    private Exception CreateMySqlException(int errorCode, string message)
    {
      return new Exception(message, new Exception($"MySQL Error Code: {errorCode}"));
    }

    [SetUp]
    public void Setup()
    {
      _serviceProviderMock = new Mock<IServiceProvider>();
      _appSettingsMock = new Mock<IOptions<AppSettings>>();
      _sqlExecutorMock = new Mock<IDatabaseExecutor>();
      _transformerMock = new Mock<IJsonToModelTransformer<TrainStation>>();

      _appSettingsMock.Setup(x => x.Value).Returns(new AppSettings
      {
        DefaultModel = typeof(TrainStation).AssemblyQualifiedName
      });

      // Mock the GetService method to return our mocked transformer
      _serviceProviderMock
        .Setup(x => x.GetService(It.Is<Type>(t => t.Name.Contains("IJsonToModelTransformer"))))
        .Returns(_transformerMock.Object);

      // Create a mock train station object for the transformer to return
      _mockTrainStation = TrainStationTestData.GetSimpleTrainStation().Station;

      // Mock the transformer's Transform method
      _transformerMock
        .Setup(x => x.Transform(It.IsAny<JsonElement>()))
        .Returns(_mockTrainStation);

      // Create mock SQL queries that would be returned by SqlInsertGenerator
      _mockSqlQueries = new List<(string, object)>
      {
          ("INSERT INTO Trains (G, L) VALUES ('ICE', 'DB123');", _mockTrainStation.Departures[0].Train),
          ("INSERT INTO Departures (DepartureStationName, DestinationStationName, Platform) VALUES ('Berlin', 'Munich', '1');", _mockTrainStation.Departures[0]),
          ("INSERT INTO TrainStations (Name) VALUES ('Berlin Hbf');", _mockTrainStation)
      };

      _controller = new LoadController(
        _serviceProviderMock.Object,
        _appSettingsMock.Object,
        _sqlExecutorMock.Object
      );
    }

    [Test]
    public void Post_WhenDuplicateData_SkipsAndContinues()
    {
      // Arrange
      var jsonData = TrainStationTestData.GetValidTrainStationJson();

      // We need to mock the SqlInsertGenerator behavior indirectly since it's static
      // This is done by intercepting the ExecuteAndReturnId call with the expected SQL query
      _sqlExecutorMock
        .Setup(x => x.ExecuteAndReturnId(It.Is<string>(sql => sql.Contains("INSERT"))))
        .Throws(CreateMySqlException(1062, "Duplicate entry 'Berlin Hbf' for key 'UK_TrainStation_Name'"));

      // Act
      var result = _controller.Post(jsonData);

      // Assert
      Assert.That(result, Is.TypeOf<OkObjectResult>());
      var okResult = (OkObjectResult)result;
      Assert.That(okResult.Value?.ToString(), Does.Contain("Skipped").IgnoreCase);
      Assert.That(okResult.Value?.ToString(), Does.Contain("duplicate").IgnoreCase);
    }

    [Test]
    public void Post_WhenNonDuplicateData_ReturnsOk()
    {
      // Arrange
      var jsonData = TrainStationTestData.GetNewTrainStationJson();

      // Setup the mock to return success for each database operation
      _sqlExecutorMock
        .Setup(x => x.ExecuteAndReturnId(It.IsAny<string>()))
        .Returns(1);

      _sqlExecutorMock
        .Setup(x => x.Execute(It.IsAny<string>()))
        .Verifiable();

      // Act
      var result = _controller.Post(jsonData);

      // Assert
      Assert.That(result, Is.TypeOf<OkObjectResult>());
      var okResult = (OkObjectResult)result;
      Assert.That(okResult.Value?.ToString(), Does.Contain("processed").IgnoreCase);
    }

    [Test]
    public void Post_WhenOtherDatabaseError_ReturnsInternalServerError()
    {
      // Arrange
      var jsonData = TrainStationTestData.GetEmptyTrainStationJson();

      _sqlExecutorMock
        .Setup(x => x.ExecuteAndReturnId(It.IsAny<string>()))
        .Throws(CreateMySqlException(1213, "Deadlock found when trying to get lock"));

      // Act
      var result = _controller.Post(jsonData);

      // Assert
      Assert.That(result, Is.TypeOf<ObjectResult>());
      var objectResult = (ObjectResult)result;
      Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }
  }
}
