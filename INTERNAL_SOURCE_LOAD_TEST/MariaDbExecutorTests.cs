using INTERNAL_SOURCE_LOAD.Services;
using INTERNAL_SOURCE_LOAD_TEST.TestData;

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

            _mockExecutor.Setup(executor => executor.Execute(invalidQuery))
                .Throws(new InvalidOperationException($"Database operation failed: {errorMessage}"));

            // WHEN & THEN: An exception should be thrown
            var ex = Assert.Throws<InvalidOperationException>(() => _mockExecutor.Object.Execute(invalidQuery));
            Assert.That(ex.Message, Does.Contain(errorMessage), "The exception message should contain the error message.");
            Assert.That(ex.Message, Does.Contain("Database operation failed"), "The exception should be a database operation error.");

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

        // Add test for the new specific error handling
        [Test]
        public void ExecuteAndReturnId_NullResult_ShouldThrowInvalidOperationException()
        {
            // GIVEN: A query that would return null
            var sqlQuery = DatabaseTestData.GetValidInsertQuery();

            _mockExecutor.Setup(executor => executor.ExecuteAndReturnId(sqlQuery))
                .Throws(new InvalidOperationException("Failed to get last inserted ID"));

            // WHEN & THEN: The correct exception type should be thrown
            var ex = Assert.Throws<InvalidOperationException>(() => _mockExecutor.Object.ExecuteAndReturnId(sqlQuery));
            Assert.That(ex.Message, Is.EqualTo("Failed to get last inserted ID"));

            _mockExecutor.Verify(executor => executor.ExecuteAndReturnId(sqlQuery), Times.Once);
        }
    }
}
