namespace INTERNAL_SOURCE_LOAD_TEST.TestData
{
    public class DatabaseTestData : TestDataFixture
    {
        public static string GetValidInsertQuery()
        {
            return "INSERT INTO test_table (name) VALUES ('Test Name');";
        }

        public static string GetAnotherValidInsertQuery()
        {
            return "INSERT INTO test_table (name) VALUES ('Another Test Name');";
        }

        public static string GetInvalidInsertQuery()
        {
            return "INSERT INTO non_existing_table (name) VALUES ('Invalid Test');";
        }

        public static string GetInvalidQueryErrorMessage()
        {
            return "Invalid query";
        }
    }
}