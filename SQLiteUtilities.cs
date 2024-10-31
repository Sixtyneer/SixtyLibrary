using System.Data;
using System.Data.SQLite;

namespace SixtyLibrary
{
    // Define result codes for easier interpretation
    public enum SQLiteResultCode
    {
        Success,
        Error
    }

    //Local DataBase file handling
    public static class SQLiteUtilities
    {
        // Stores the SQLite connection string
        private static string? _connectionString;

        // Set connection string (you could have other methods to manage this if needed)
        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Executes a non-query (INSERT, UPDATE, DELETE) and returns a result code
        public static (SQLiteResultCode Result, int RowsAffected, string? ErrorMessage) ExecuteNonQuery(string sql)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        return (SQLiteResultCode.Success, rowsAffected, null);
                    }
                }
            }
            catch (Exception ex)
            {
                return (SQLiteResultCode.Error, 0, ex.Message);
            }
        }

        // Executes a query and returns the result as a DataTable along with a result code
        public static (SQLiteResultCode Result, DataTable? Data, string? ErrorMessage) ExecuteQuery(string sql)
        {
            var dataTable = new DataTable();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                        return (SQLiteResultCode.Success, dataTable, null);
                    }
                }
            }
            catch (Exception ex)
            {
                return (SQLiteResultCode.Error, null, ex.Message);
            }
        }

        // Executes a scalar query and returns a single value with a result code
        public static (SQLiteResultCode Result, object? Value, string? ErrorMessage) ExecuteScalar(string sql)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        object value = command.ExecuteScalar();
                        return (SQLiteResultCode.Success, value, null);
                    }
                }
            }
            catch (Exception ex)
            {
                return (SQLiteResultCode.Error, null, ex.Message);
            }
        }

        // Executes multiple commands within a transaction, returning a result code
        public static (SQLiteResultCode Result, string? ErrorMessage) ExecuteTransaction(Action<SQLiteConnection, SQLiteTransaction> action)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            action(connection, transaction);
                            transaction.Commit();
                            return (SQLiteResultCode.Success, null);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return (SQLiteResultCode.Error, ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (SQLiteResultCode.Error, ex.Message);
            }
        }
    }
}
