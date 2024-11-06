using System.Data.SqlClient;
using System.Data;

namespace SixtyLibrary
{
    public static class MSSQLUtilities
    {
        private static string? _connectionString;

        public static void SetConnectionString(string server, int port, string database, string userID, string password)
        {
            _connectionString = $"Server={server},{port}; Database={database}; User Id={userID}; Password={password};";
        }
        // Executes a non-query command
        public static (bool Success, string? ErrorMessage) ExecuteNonQuery(string sql)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                        return (true, null);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Executes a query and returns a DataTable
        public static (bool Success, DataTable? Data, string? ErrorMessage) ExecuteQuery(string sql)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                        return (true, dataTable, null);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        // Executes a scalar query to retrieve a single value for - Aggregates (SUM, COUNT, AVG)
        public static (bool Success, object? Value, string? ErrorMessage) ExecuteScalar(string sql)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        object? value = command.ExecuteScalar();
                        return (true, value, null);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
