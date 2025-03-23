using INTERNAL_SOURCE_LOAD.Models;
using INTERNAL_SOURCE_LOAD.Services;
using INTERNAL_SOURCE_LOAD_TEST.TestData;
using MySql.Data.MySqlClient;
using System;

namespace INTERNAL_SOURCE_LOAD_TEST
{
    [TestFixture]
    public class DataPersistenceServiceTests
    {
        private DataPersistenceService _service = null!;
        private Mock<IDatabaseExecutor> _databaseExecutorMock = null!;
        private TrainStation _mockTrainStation = null!;

        private Exception CreateMySqlDuplicateKeyException()
        {
            // Use a generic exception with the right properties for testing
            // This simulates the behavior we check for in IsDuplicateKeyError
            return new Exception("Duplicate entry 'Berlin Hbf' for key 'UK_TrainStation_Name'");
        }

        private Exception CreateGenericDuplicateKeyException()
        {
            return new Exception("Duplicate entry 'Berlin Hbf' for key 'UK_TrainStation_Name'");
        }

        private Exception CreateNestedMySqlException()
        {
            return new Exception("Outer exception", new Exception("MySQL Error Code: 1062"));
        }

        private Exception CreateOtherDatabaseException()
        {
            // Now using InvalidOperationException for database errors
            return new InvalidOperationException("Database operation failed: Database connection error");
        }

        [SetUp]
        public void Setup()
        {
            _databaseExecutorMock = new Mock<IDatabaseExecutor>();
            _service = new DataPersistenceService(_databaseExecutorMock.Object);
            _mockTrainStation = TrainStationTestData.GetSimpleTrainStation().Station;
        }

        [Test]
        public void PersistData_SuccessfulInsertion_ReturnsSuccessResult()
        {
            // Arrange
            string modelTypeName = typeof(TrainStation).AssemblyQualifiedName!;
            _databaseExecutorMock
                .Setup(x => x.ExecuteAndReturnId(It.IsAny<string>()))
                .Returns(1);

            _databaseExecutorMock
                .Setup(x => x.Execute(It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = _service.PersistData(_mockTrainStation, modelTypeName);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.SkippedDuplicates, Is.EqualTo(0));
            Assert.That(result.Message, Does.Contain("Data processed"));
            _databaseExecutorMock.Verify(x => x.ExecuteAndReturnId(It.IsAny<string>()), Times.AtLeastOnce);
            _databaseExecutorMock.Verify(x => x.Execute(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Test]
        public void PersistData_DuplicateKeyInMainInsert_SkipsAndContinues()
        {
            // Arrange
            string modelTypeName = typeof(TrainStation).AssemblyQualifiedName!;

            // First call throws duplicate key, subsequent calls succeed
            var callCount = 0;
            _databaseExecutorMock
                .Setup(x => x.ExecuteAndReturnId(It.IsAny<string>()))
                .Returns(() =>
                {
                    if (callCount++ == 0)
                        throw CreateMySqlDuplicateKeyException();
                    return 1;
                });

            _databaseExecutorMock
                .Setup(x => x.Execute(It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = _service.PersistData(_mockTrainStation, modelTypeName);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.SkippedDuplicates, Is.GreaterThan(0));
            Assert.That(result.Message, Does.Contain("Skipped"));
        }

        [Test]
        public void PersistData_DuplicateKeyInForeignKeyUpdate_SkipsAndContinues()
        {
            // Arrange
            string modelTypeName = typeof(TrainStation).AssemblyQualifiedName!;

            _databaseExecutorMock
                .Setup(x => x.ExecuteAndReturnId(It.IsAny<string>()))
                .Returns(1);

            // Foreign key update throws duplicate key exception
            _databaseExecutorMock
                .Setup(x => x.Execute(It.IsAny<string>()))
                .Throws(CreateGenericDuplicateKeyException());

            // Act
            var result = _service.PersistData(_mockTrainStation, modelTypeName);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.SkippedDuplicates, Is.GreaterThan(0));
            Assert.That(result.Message, Does.Contain("Skipped"));
        }

        [Test]
        public void PersistData_DatabaseError_ReturnsErrorResult()
        {
            // Arrange
            string modelTypeName = typeof(TrainStation).AssemblyQualifiedName!;

            _databaseExecutorMock
                .Setup(x => x.ExecuteAndReturnId(It.IsAny<string>()))
                .Throws(CreateOtherDatabaseException());

            // Act
            var result = _service.PersistData(_mockTrainStation, modelTypeName);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("Database error"));
            Assert.That(result.Errors.Count, Is.GreaterThan(0));
        }

        [Test]
        public void IsDuplicateKeyError_MySqlException_ReturnsTrue()
        {
            // Act
            var result = _service.IsDuplicateKeyError(CreateMySqlDuplicateKeyException());

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsDuplicateKeyError_MessageContainsDuplicateEntry_ReturnsTrue()
        {
            // Act
            var result = _service.IsDuplicateKeyError(CreateGenericDuplicateKeyException());

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsDuplicateKeyError_InnerExceptionContainsErrorCode_ReturnsTrue()
        {
            // Act
            var result = _service.IsDuplicateKeyError(CreateNestedMySqlException());

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsDuplicateKeyError_OtherException_ReturnsFalse()
        {
            // Act
            var result = _service.IsDuplicateKeyError(CreateOtherDatabaseException());

            // Assert
            Assert.That(result, Is.False);
        }

        // Add test for the new InvalidOperationException database error handling
        [Test]
        public void PersistData_GetLastInsertIdError_ReturnsErrorResult()
        {
            // Arrange
            string modelTypeName = typeof(TrainStation).AssemblyQualifiedName!;

            _databaseExecutorMock
                .Setup(x => x.ExecuteAndReturnId(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Failed to get last inserted ID"));

            // Act
            var result = _service.PersistData(_mockTrainStation, modelTypeName);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("Database error"));
            Assert.That(result.Errors.Count, Is.GreaterThan(0));
        }
    }
}