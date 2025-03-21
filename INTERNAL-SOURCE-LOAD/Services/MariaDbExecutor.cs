namespace INTERNAL_SOURCE_LOAD.Services
{
    using MySql.Data.MySqlClient;

    public class MariaDbExecutor : IDatabaseExecutor
    {
        private readonly string _connectionString;

        public MariaDbExecutor(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Executes a SQL query on the MariaDB database.
        /// </summary>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        public void Execute(string sqlQuery)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }
        public long ExecuteAndReturnId(string sqlQuery)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();

            // Retrieve the last inserted ID
            command.CommandText = "SELECT LAST_INSERT_ID();";
            return Convert.ToInt64(command.ExecuteScalar());
        }

    }
}
