using System.Text.Json;
using INTERNAL_SOURCE_LOAD;
using INTERNAL_SOURCE_LOAD.Controllers;
using INTERNAL_SOURCE_LOAD.Models;
using INTERNAL_SOURCE_LOAD.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework.Legacy;
using INTERNAL_SOURCE_LOAD_TEST.TestData;
using static INTERNAL_SOURCE_LOAD_TEST.TestData.SqlTestData;

namespace INTERNAL_SOURCE_LOAD_TEST
{
    [TestFixture]
    public class SqlInsertGeneratorTests
    {
        [Test]
        public void GenerateInsertQueries_GivenValidSimpleObject_ReturnsExpectedQuery()
        {
            // Given
            string validQuery = GetValidInsertQuery();
            var simpleModel = GetSimpleModel();

            // When
            var result = SqlInsertGenerator.GenerateInsertQueries("SimpleTable", simpleModel);

            // Then
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(validQuery, Is.EqualTo(result[0].Query));
        }

        [Test]
        public void GenerateInsertQueries_GivenNestedObject_ReturnsExpectedQueries()
        {
            // Given
            var (validQuery1, validQuery2, validQuery3) = GetNestedObjectQueries();
            var complexModel = GetComplexModel();

            // When
            var result = SqlInsertGenerator.GenerateInsertQueries("ComplexModel", complexModel);

            // Then
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(validQuery1, Is.EqualTo(result[2].Query));
            Assert.That(validQuery2, Is.EqualTo(result[0].Query));
            Assert.That(validQuery3, Is.EqualTo(result[1].Query));
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
            var simpleModels = GetSimpleModelList();

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
            var trainStation = TrainStationTestData.GetComplexTrainStation();
            var departure = trainStation.Departures[0];
            var modelIds = TrainStationTestData.GetModelIds(trainStation);

            string expectedQuery = GetValidUpdateQuery();

            // When
            var result = SqlInsertGenerator.GenerateUpdateForeignKeysQueries(departure, modelIds);

            // Then
            Assert.That(result, Is.Not.Empty);
            Assert.That(result[0], Is.EqualTo(expectedQuery));
        }

        [Test]
        public void GenerateUpdateForeignKeys_GivenMultipleDeparturesWithDifferentTrains_ReturnsCorrectUpdateQueries()
        {
            // Given
            var trainStation = TrainStationTestData.GetComplexTrainStation();
            var modelIds = TrainStationTestData.GetModelIds(trainStation);
            var expectedQueries = GetMultipleUpdateQueries();

            // When
            var results = trainStation.Departures.Select(d =>
                SqlInsertGenerator.GenerateUpdateForeignKeysQueries(d, modelIds)[0]).ToList();

            // Then
            Assert.That(results.Count, Is.EqualTo(3), "Should generate 3 update queries");
            for (int i = 0; i < results.Count; i++)
            {
                Assert.That(results[i], Is.EqualTo(expectedQueries[i]),
                    $"Departure {i + 1} should have correct foreign key update query");
            }
        }

        [Test]
        public void GenerateUpdateForeignKeys_GivenDepartureWithTrainStation_ReturnsCorrectUpdateQueries()
        {
            // Given
            var (trainStation, train, departure) = TrainStationTestData.GetSimpleTrainStation();
            var modelIds = TrainStationTestData.GetSimpleModelIds(train, departure, trainStation);
            var expectedQueries = GetTrainStationUpdateQueries();

            // When
            var result = SqlInsertGenerator.GenerateUpdateForeignKeysQueries(departure, modelIds);

            // Then
            Assert.That(result.Count, Is.EqualTo(2), "Should generate update queries for both Train and TrainStation");
            Assert.That(result, Is.EquivalentTo(expectedQueries), "Should generate correct update queries for both foreign keys");
        }
    }
}
