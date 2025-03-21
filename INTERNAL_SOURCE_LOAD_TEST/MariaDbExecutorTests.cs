using System.Text.Json;
using INTERNAL_SOURCE_LOAD;
using INTERNAL_SOURCE_LOAD.Controllers;
using INTERNAL_SOURCE_LOAD.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using NUnit.Framework.Legacy;


namespace INTERNAL_SOURCE_LOAD_TEST
{
  [TestFixture]
  public class MariaDbExecutorTests
    {
        private Mock<IDatabaseExecutor> _mockExecutor;

        [SetUp]
        public void SetUp()
        {
            // Create a mock for the IDatabaseExecutor
            _mockExecutor = new Mock<IDatabaseExecutor>();
        }

        [Test]
        public void Execute_ValidQuery_ShouldCallExecuteOnce()
        {
            // GIVEN: A valid SQL query
            var sqlQuery = "INSERT INTO test_table (name) VALUES ('Test Name');";

            // WHEN: The Execute method is called
            _mockExecutor.Object.Execute(sqlQuery);

            // THEN: Verify the Execute method is called exactly once
            _mockExecutor.Verify(executor => executor.Execute(sqlQuery), Times.Once, "The Execute method should be called exactly once.");
        }

        [Test]
        public void ExecuteAndReturnId_ValidQuery_ShouldReturnMockedId()
        {
            // GIVEN: A valid SQL query and a mocked ID
            var sqlQuery = "INSERT INTO test_table (name) VALUES ('Another Test Name');";
            var expectedId = 123;

            _mockExecutor.Setup(executor => executor.ExecuteAndReturnId(sqlQuery)).Returns(expectedId);

            // WHEN: The ExecuteAndReturnId method is called
            var actualId = _mockExecutor.Object.ExecuteAndReturnId(sqlQuery);

            // THEN: The returned ID should match the mocked ID
            Assert.That(actualId, Is.EqualTo(expectedId), "The returned ID should match the mocked ID.");

            // Verify the method is called exactly once
            _mockExecutor.Verify(executor => executor.ExecuteAndReturnId(sqlQuery), Times.Once, "The ExecuteAndReturnId method should be called exactly once.");
        }

        [Test]
        public void Execute_InvalidQuery_ShouldThrowException()
        {
            // GIVEN: An invalid SQL query
            var invalidQuery = "INSERT INTO non_existing_table (name) VALUES ('Invalid Test');";

            _mockExecutor.Setup(executor => executor.Execute(invalidQuery)).Throws(new Exception("Invalid query"));

            // WHEN & THEN: An exception should be thrown
            var ex = Assert.Throws<Exception>(() => _mockExecutor.Object.Execute(invalidQuery));
            Assert.That(ex.Message, Is.EqualTo("Invalid query"), "The exception message should match the mocked exception.");

            // Verify the method is called exactly once
            _mockExecutor.Verify(executor => executor.Execute(invalidQuery), Times.Once, "The Execute method should be called exactly once.");
        }
    }
}
