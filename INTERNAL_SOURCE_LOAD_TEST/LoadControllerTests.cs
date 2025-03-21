using System.Text.Json;
using INTERNAL_SOURCE_LOAD;
using INTERNAL_SOURCE_LOAD.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework.Legacy;


namespace INTERNAL_SOURCE_LOAD_TEST
{
  [TestFixture]
  public class TrainJsonToSqlTransformerTests
  {
  //  private TrainJsonToSqlTransformer _transformer;
  //  private LoadController _controller;

  //  [SetUp]
  //  public void Setup()
  //  {
  //    _transformer = new TrainJsonToSqlTransformer();
  //    List<IJsonToSqlTransformer> transformers = new List<IJsonToSqlTransformer> { new TrainJsonToSqlTransformer() };
  //    _controller = new LoadController(transformers);
  //  }

  //  [Test]
  //  public void CanHandle_ValidJsonStructure_ReturnsTrue()
  //  {
  //    // Arrange
  //    var jsonString = @"
  //  {
  //    ""name"": ""Yverdon-les-Bains"",
  //    ""departures"": [
  //      {
  //        ""departureStationName"": ""Yverdon-les-Bains"",
  //        ""destinationStationName"": ""Lausanne"",
  //        ""viaStationNames"": [
  //          """"
  //        ],
  //        ""departureTime"": ""2024-12-09T08:00:00"",
  //        ""train"": {
  //          ""g"": ""IC"",
  //          ""l"": ""5""
  //        },
  //        ""platform"": ""2"",
  //        ""sector"": null
  //      },
  //      {
  //        ""departureStationName"": ""Yverdon-les-Bains"",
  //        ""destinationStationName"": ""Fribourg/Freiburg"",
  //        ""viaStationNames"": [
  //          ""Yverdon-Champ Pittet"",
  //          ""Yvonand"",
  //          ""Cheyres"",
  //          ""Payerne""
  //        ],
  //        ""departureTime"": ""2024-12-09T13:18:00"",
  //        ""train"": {
  //          ""g"": ""S"",
  //          ""l"": ""30""
  //        },
  //        ""platform"": ""3"",
  //        ""sector"": ""D""
  //      },
  //      {
  //        ""departureStationName"": ""Yverdon-les-Bains"",
  //        ""destinationStationName"": ""Genève Aéroport"",
  //        ""viaStationNames"": [
  //          ""Morges""
  //        ],
  //        ""departureTime"": ""2024-12-09T16:45:00"",
  //        ""train"": {
  //          ""g"": ""IC"",
  //          ""l"": ""5""
  //        },
  //        ""platform"": ""2"",
  //        ""sector"": null
  //      },
  //      {
  //        ""departureStationName"": ""Yverdon-les-Bains"",
  //        ""destinationStationName"": ""Rorschar"",
  //        ""viaStationNames"": [
  //          ""Neuchâtel"",
  //          ""Biel/Bienne"",
  //          ""Olten"",
  //          ""St. Gallen""
  //        ],
  //        ""departureTime"": ""2024-12-09T23:00:00"",
  //        ""train"": {
  //          ""g"": ""IC"",
  //          ""l"": ""5""
  //        },
  //        ""platform"": ""1"",
  //        ""sector"": null
  //      }
  //    ]
  //  }";

  //    var jsonDocument = JsonDocument.Parse(jsonString);
  //    var jsonElement = jsonDocument.RootElement;

  //    // Act
  //    var result = _transformer.CanHandle(jsonElement);

  //    // Assert
  //    ClassicAssert.True(result);
  //  }
  //  [Test]
  //  public void Transform_ValidJson_ReturnsExpectedSql()
  //  {
  //    // Arrange
  //    var jsonString = @"
  //  {
  //    ""name"": ""Yverdon-les-Bains"",
  //    ""departures"": [
  //      {
  //        ""departureStationName"": ""Yverdon-les-Bains"",
  //        ""destinationStationName"": ""Lausanne"",
  //        ""viaStationNames"": [
  //          """"
  //        ],
  //        ""departureTime"": ""2024-12-09T08:00:00"",
  //        ""train"": {
  //          ""g"": ""IC"",
  //          ""l"": ""5""
  //        },
  //        ""platform"": ""2"",
  //        ""sector"": null
  //      },
  //      {
  //        ""departureStationName"": ""Yverdon-les-Bains"",
  //        ""destinationStationName"": ""Fribourg/Freiburg"",
  //        ""viaStationNames"": [
  //          ""Yverdon-Champ Pittet"",
  //          ""Yvonand"",
  //          ""Cheyres"",
  //          ""Payerne""
  //        ],
  //        ""departureTime"": ""2024-12-09T13:18:00"",
  //        ""train"": {
  //          ""g"": ""S"",
  //          ""l"": ""30""
  //        },
  //        ""platform"": ""3"",
  //        ""sector"": ""D""
  //      }
  //    ]
  //  }";

  //    var expectedSql =
  //        "INSERT INTO TrainDepartures (DepartureStationName, DestinationStationName, ViaStationNames, DepartureTime, TrainGroup, TrainLine, Platform, Sector) " +
  //        "VALUES ('Yverdon-les-Bains', 'Lausanne', '', '2024-12-09 08:00:00', 'IC', '5', '2', NULL);\n" +
  //        "INSERT INTO TrainDepartures (DepartureStationName, DestinationStationName, ViaStationNames, DepartureTime, TrainGroup, TrainLine, Platform, Sector) " +
  //        "VALUES ('Yverdon-les-Bains', 'Fribourg/Freiburg', 'Yverdon-Champ Pittet,Yvonand,Cheyres,Payerne', '2024-12-09 13:18:00', 'S', '30', '3', 'D');";

  //    var jsonDocument = JsonDocument.Parse(jsonString);
  //    var jsonElement = jsonDocument.RootElement;

  //    var transformer = new TrainJsonToSqlTransformer();

  //    // Act
  //    var result = transformer.Transform(jsonElement);

  //    // Assert
  //    //ClassicAssert.AreEqual(expectedSql, result);
  //    Assert.That(expectedSql, Is.EqualTo(result));
  //  }
  //  [Test]
  //  public void Post_ValidJson_UsesTrainJsonToSqlTransformerAndReturns201()
  //  {
  //    // Arrange
  //    var jsonString = @"
  //          {
  //            ""name"": ""Yverdon-les-Bains"",
  //            ""departures"": [
  //              {
  //                ""departureStationName"": ""Yverdon-les-Bains"",
  //                ""destinationStationName"": ""Lausanne"",
  //                ""viaStationNames"": [],
  //                ""departureTime"": ""2024-12-09T08:00:00"",
  //                ""train"": {
  //                  ""g"": ""IC"",
  //                  ""l"": ""5""
  //                },
  //                ""platform"": ""2"",
  //                ""sector"": null
  //              }
  //            ]
  //          }";

  //    var jsonDocument = JsonDocument.Parse(jsonString);
  //    var jsonElement = jsonDocument.RootElement;

  //    // Act
  //    var response = _controller.Post(jsonElement);

  //    // Assert
  //    ClassicAssert.IsInstanceOf<ObjectResult>(response);
  //    var objectResult = (ObjectResult)response;
  //    ClassicAssert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);
  //    ClassicAssert.AreEqual("Data loaded successfully.", objectResult.Value);
  //  }
  //  [Test]
  //  public void Post_InvalidJson_ReturnsBadRequest()
  //  {
  //    // Arrange
  //    string jsonString = @"{ ""invalid"": true }";
  //    JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
  //    JsonElement jsonElement = jsonDocument.RootElement;

  //    // Act
  //    var response = _controller.Post(jsonElement);

  //    // Assert
  //    ClassicAssert.IsInstanceOf<BadRequestObjectResult>(response);
  //    var badRequestResult = (BadRequestObjectResult)response;
  //    ClassicAssert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
  //    ClassicAssert.AreEqual("No suitable transformer found for the provided data.", badRequestResult.Value);
  //  }
  //  [Test]
  //  public void Post_ExceptionDuringProcessing_ReturnsInternalServerError()
  //  {
  //    // Arrange
  //    var transformerMock = new Mock<IJsonToSqlTransformer>();
  //    transformerMock.Setup(t => t.CanHandle(It.IsAny<JsonElement>())).Returns(true);
  //    transformerMock.Setup(t => t.Transform(It.IsAny<JsonElement>())).Throws(new Exception("Unexpected error"));

  //    var controller = new LoadController(new List<IJsonToSqlTransformer> { transformerMock.Object });

  //    var jsonString = @"
  //      {
  //        ""name"": ""Yverdon-les-Bains"",
  //        ""departures"": []
  //      }";

  //    var jsonDocument = JsonDocument.Parse(jsonString);
  //    var jsonElement = jsonDocument.RootElement;

  //    // Act
  //    var response = controller.Post(jsonElement);

  //    // Assert
  //    ClassicAssert.IsInstanceOf<ObjectResult>(response);
  //    var objectResult = (ObjectResult)response;
  //    ClassicAssert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
  //    ClassicAssert.AreEqual("Error loading data: Unexpected error", objectResult.Value);
  //  }
  }
}
