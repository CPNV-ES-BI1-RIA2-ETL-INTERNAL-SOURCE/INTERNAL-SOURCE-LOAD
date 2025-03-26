namespace INTERNAL_SOURCE_LOAD.Services
{
    using MySql.Data.MySqlClient;
    using System;
    using System.Data.Common;

    public class MariaDbExecutor : IDatabaseExecutor
    {
        private readonly string _connectionString;

        public MariaDbExecutor(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Executes a SQL query on the MariaDB database.
        /// </summary>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        public void Execute(string sqlQuery)
        {
            if (string.IsNullOrEmpty(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                using var command = new MySqlCommand(sqlQuery, connection);
                command.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                // Let MySqlException propagate up directly for specific error handling
                throw;
            }
            catch (Exception ex)
            {
                // Convert generic exceptions to more specific DbException
                throw new InvalidOperationException($"Database operation failed: {ex.Message}", ex);
            }
        }

        public long ExecuteAndReturnId(string sqlQuery)
        {
            if (string.IsNullOrEmpty(sqlQuery))
                throw new ArgumentNullException(nameof(sqlQuery));

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                using var command = new MySqlCommand(sqlQuery, connection);
                command.ExecuteNonQuery();

                // Retrieve the last inserted ID
                command.CommandText = "SELECT LAST_INSERT_ID();";
                var result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    throw new InvalidOperationException("Failed to get last inserted ID");

                return Convert.ToInt64(result);
            }
            catch (MySqlException ex)
            {
                // Let MySqlException propagate up directly for specific error handling
                throw;
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidOperationException("Failed to convert database ID to expected type", ex);
            }
            catch (Exception ex)
            {
                // Convert generic exceptions to more specific DbException
                throw new InvalidOperationException($"Database operation failed: {ex.Message}", ex);
            }
        }
    }
}
