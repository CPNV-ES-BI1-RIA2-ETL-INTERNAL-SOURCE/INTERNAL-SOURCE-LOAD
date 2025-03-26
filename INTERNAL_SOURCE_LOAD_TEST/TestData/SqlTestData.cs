using INTERNAL_SOURCE_LOAD.Models;

namespace INTERNAL_SOURCE_LOAD_TEST.TestData
{
    public class SqlTestData : TestDataFixture
    {
        public static string GetValidInsertQuery()
        {
            return "INSERT INTO SimpleModel (Id, Name) VALUES (1, 'Test');";
        }

        public static string GetValidUpdateQuery()
        {
            return "UPDATE Departures SET TrainID = 42 WHERE Id = 43";
        }

        public static (string Parent, string Child1, string Child2) GetNestedObjectQueries()
        {
            return (
                "INSERT INTO ComplexModel (Id, Name) VALUES (1, 'Parent');",
                "INSERT INTO SimpleModel (Id, Name) VALUES (2, 'Child1');",
                "INSERT INTO SimpleModel (Id, Name) VALUES (3, 'Child2');"
            );
        }

        public static List<string> GetMultipleUpdateQueries()
        {
            return new List<string>
            {
                "UPDATE Departures SET TrainID = 42 WHERE Id = 43",
                "UPDATE Departures SET TrainID = 44 WHERE Id = 45",
                "UPDATE Departures SET TrainID = 46 WHERE Id = 47"
            };
        }

        public static List<string> GetTrainStationUpdateQueries()
        {
            return new List<string>
            {
                "UPDATE Departures SET TrainID = 42 WHERE Id = 123",
                "UPDATE Departures SET TrainStationID = 55 WHERE Id = 123"
            };
        }

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

        public static SimpleModel GetSimpleModel()
        {
            return new SimpleModel { Id = 1, Name = "Test" };
        }

        public static ComplexModel GetComplexModel()
        {
            return new ComplexModel
            {
                Id = 1,
                Name = "Parent",
                Items = new List<SimpleModel>
                {
                    new SimpleModel { Id = 2, Name = "Child1" },
                    new SimpleModel { Id = 3, Name = "Child2" }
                }
            };
        }

        public static List<SimpleModel> GetSimpleModelList()
        {
            return new List<SimpleModel>
            {
                new SimpleModel { Id = 1, Name = "Test" }
            };
        }
    }
}