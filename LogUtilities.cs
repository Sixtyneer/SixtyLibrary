namespace SixtyLibrary
{
    public static class LogUtilities
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log");
        private static readonly object lockObj = new object();

        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        public static string Log(string message, LogLevel level = LogLevel.Info)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            lock (lockObj)  // Thread safety
            {
                try
                {
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                    return ""; // No error
                }
                catch (Exception ex)
                {
                    return $"Logging failed: {ex.Message}";
                }
            }
        }
    }
}
