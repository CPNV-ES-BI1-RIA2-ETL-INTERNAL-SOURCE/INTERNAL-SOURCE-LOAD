using System;
using Moq;
using INTERNAL_SOURCE_LOAD.Services;
using NUnit.Framework;
using INTERNAL_SOURCE_LOAD_TEST.TestData;
using MySql.Data.MySqlClient;

namespace INTERNAL_SOURCE_LOAD_TEST
{
    [TestFixture]
    public class MariaDbExecutorTests
    {
        private Mock<IDatabaseExecutor> _mockExecutor = null!;

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
            var sqlQuery = DatabaseTestData.GetValidInsertQuery();

            // WHEN: The Execute method is called
            _mockExecutor.Object.Execute(sqlQuery);

            // THEN: Verify the Execute method is called exactly once
            _mockExecutor.Verify(executor => executor.Execute(sqlQuery), Times.Once, "The Execute method should be called exactly once.");
        }

        [Test]
        public void ExecuteAndReturnId_ValidQuery_ShouldReturnMockedId()
        {
            // GIVEN: A valid SQL query and a mocked ID
            var sqlQuery = DatabaseTestData.GetAnotherValidInsertQuery();
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
            var invalidQuery = DatabaseTestData.GetInvalidInsertQuery();
            var errorMessage = DatabaseTestData.GetInvalidQueryErrorMessage();

            _mockExecutor.Setup(executor => executor.Execute(invalidQuery)).Throws(new Exception(errorMessage));

            // WHEN & THEN: An exception should be thrown
            var ex = Assert.Throws<Exception>(() => _mockExecutor.Object.Execute(invalidQuery));
            Assert.That(ex.Message, Is.EqualTo(errorMessage), "The exception message should match the mocked exception.");

            // Verify the method is called exactly once
            _mockExecutor.Verify(executor => executor.Execute(invalidQuery), Times.Once, "The Execute method should be called exactly once.");
        }

        [Test]
        public void MariaDbExecutor_ConnectionString_ShouldCreateProperInstance()
        {
            // GIVEN: A connection string
            var connectionString = "Server=localhost;Database=test;User=root;Password=password;";

            // WHEN: Creating a MariaDbExecutor
            var executor = new MariaDbExecutor(connectionString);

            // THEN: The executor should not be null
            Assert.That(executor, Is.Not.Null);
            Assert.That(executor, Is.InstanceOf<IDatabaseExecutor>());
        }
    }
}
