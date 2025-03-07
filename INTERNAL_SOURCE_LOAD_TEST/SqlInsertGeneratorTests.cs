using System.Text.Json;
using INTERNAL_SOURCE_LOAD;
using INTERNAL_SOURCE_LOAD.Controllers;
using INTERNAL_SOURCE_LOAD.Models;
using INTERNAL_SOURCE_LOAD.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework.Legacy;


namespace INTERNAL_SOURCE_LOAD_TEST
{
    [TestFixture]
    public class SqlInsertGeneratorTests
    {
        public class SimpleModel
        {
            public int Id { get; set; }
            public required string Name { get; set; }
        }

        public class ComplexModel
        {
            public int Id { get; set; }
            public required string Name { get; set; }
            public List<SimpleModel>? Items { get; set; }
        }

        [Test]
        public void GenerateInsertQueries_GivenValidSimpleObject_ReturnsExpectedQuery()
        {
            // Given
            string valideQuery = "INSERT INTO SimpleModel (Id, Name) VALUES (1, 'Test');";

            var simpleModel = new SimpleModel { Id = 1, Name = "Test" };

            // When
            var result = SqlInsertGenerator.GenerateInsertQueries("SimpleTable", simpleModel);

            // Then
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(valideQuery, Is.EqualTo(result[0].Query));
        }

        [Test]
        public void GenerateInsertQueries_GivenNestedObject_ReturnsExpectedQueries()
        {
            // Given
            string valideQuery1 = "INSERT INTO ComplexModel (Id, Name) VALUES (1, 'Parent');";
            string valideQuery2 = "INSERT INTO SimpleModel (Id, Name) VALUES (2, 'Child1');";
            string valideQuery3 = "INSERT INTO SimpleModel (Id, Name) VALUES (3, 'Child2');";
            var complexModel = new ComplexModel
            {
                Id = 1,
                Name = "Parent",
                Items = new List<SimpleModel>
                    {
                        new SimpleModel { Id = 2, Name = "Child1" },
                        new SimpleModel { Id = 3, Name = "Child2" }
                    }
            };

            // When
            var result = SqlInsertGenerator.GenerateInsertQueries("ComplexModel", complexModel);

            // Then
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(valideQuery1, Is.EqualTo(result[2].Query));
            Assert.That(valideQuery2, Is.EqualTo(result[0].Query));
            Assert.That(valideQuery3, Is.EqualTo(result[1].Query));

        }

        [Test]
        public void GenerateInsertQueries_GivenNullObject_ThrowsArgumentNullException()
        {
            // Given
            object? nullModel = null;

            // When & Then
            var exception = Assert.Throws<ArgumentNullException>(() =>
                SqlInsertGenerator.GenerateInsertQueries("Table", nullModel)
            );

            Assert.That(exception.Message, Is.EqualTo("Value cannot be null. (Parameter 'data')"));
        }

        [Test]
        public void GenerateInsertQueries_GivenTopLevelCollection_ThrowsInvalidOperationException()
        {
            // Given
            var simpleModels = new List<SimpleModel>
                {
                    new SimpleModel { Id = 1, Name = "Test" }
                };

            // When & Then
            var exception = Assert.Throws<InvalidOperationException>(() =>
                SqlInsertGenerator.GenerateInsertQueries("Table", simpleModels)
            );

            Assert.That(exception.Message, Is.EqualTo("Top-level collections are not supported for insert queries."));
        }

        [Test]
        public void GenerateUpdateForeignKeys_GivenDepartureWithTrain_ReturnsCorrectUpdateQuery()
        {
            // Given
            var train = new Train("ICE", "DB123");
            var departure = new Departure(
                DepartureStationName: "Berlin",
                DestinationStationName: "Munich",
                ViaStationNames: new List<string> { "Frankfurt" },
                DepartureTime: DateTime.Now,
                Train: train,
                Platform: "1"
            );

            var modelIds = new Dictionary<object, long>
                {
                    { train, 42 },      // Train got ID 42
                    { departure, 123 }   // Departure got ID 123
                };

            string expectedQuery = "UPDATE Departures SET TrainID = 42 WHERE Id = 123";

            // When
            var result = SqlInsertGenerator.GenerateUpdateForeignKeysQueries(departure, modelIds);
            Console.WriteLine(result);
            //run the query to see if it works


            // Then
            Assert.That(result, Is.Not.Empty);
            Assert.That(result[0], Is.EqualTo(expectedQuery));
        }

        [Test]
        public void GenerateUpdateForeignKeys_GivenMultipleDeparturesWithDifferentTrains_ReturnsCorrectUpdateQueries()
        {
            // Given
            var train1 = new Train("ICE", "DB123");
            var train2 = new Train("TGV", "FR456");
            var train3 = new Train("ICE", "DB789");

            var departure1 = new Departure(
                DepartureStationName: "Berlin",
                DestinationStationName: "Munich",
                ViaStationNames: new List<string> { "Frankfurt" },
                DepartureTime: DateTime.Now,
                Train: train1,
                Platform: "1"
            );

            var departure2 = new Departure(
                DepartureStationName: "Paris",
                DestinationStationName: "Lyon",
                ViaStationNames: new List<string> { "Dijon" },
                DepartureTime: DateTime.Now,
                Train: train2,
                Platform: "2"
            );
            var departure3 = new Departure(
                DepartureStationName: "Berlin",
                DestinationStationName: "Paris",
                ViaStationNames: new List<string> { "Frankfurt" },
                DepartureTime: DateTime.Now,
                Train: train3,
                Platform: "1"
            );

            var modelIds = new Dictionary<object, long>
                {
                    { train1, 42 },     // First train ID
                    { train2, 43 },     // Second train ID
                    { train3, 44 },     // Third train ID
                    { departure1, 123 }, // First departure ID
                    { departure2, 124 }, // Second departure ID
                    { departure3, 125 }  // Third departure ID
                };

            // Expected queries for both departures
            var expectedQueries = new List<string>
                {
                    "UPDATE Departures SET TrainID = 42 WHERE Id = 123",
                    "UPDATE Departures SET TrainID = 43 WHERE Id = 124",
                    "UPDATE Departures SET TrainID = 44 WHERE Id = 125"
                };

            // When
            var result1 = SqlInsertGenerator.GenerateUpdateForeignKeysQueries(departure1, modelIds);
            var result2 = SqlInsertGenerator.GenerateUpdateForeignKeysQueries(departure2, modelIds);
            var result3 = SqlInsertGenerator.GenerateUpdateForeignKeysQueries(departure3, modelIds);

            // Then
            Assert.That(result1.Count + result2.Count + result3.Count, Is.EqualTo(3), "Should generate 3 update queries");
            Assert.That(result1[0], Is.EqualTo(expectedQueries[0]), "First departure should reference train1");
            Assert.That(result2[0], Is.EqualTo(expectedQueries[1]), "Second departure should reference train2");
            Assert.That(result3[0], Is.EqualTo(expectedQueries[2]), "Third departure should reference train3");

            // Additional verification that the queries are different
            Assert.That(result1[0], Is.Not.EqualTo(result2[0]), "Update queries should be different for different trains");
            Assert.That(result2[0], Is.Not.EqualTo(result3[0]), "Update queries should be different for different trains");
            Assert.That(result1[0], Is.Not.EqualTo(result3[0]), "Update queries should be different for different trains");
        }
    }
}
