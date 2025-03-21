using System.Text.Json;
using INTERNAL_SOURCE_LOAD;
using INTERNAL_SOURCE_LOAD.Controllers;
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
    }
}
