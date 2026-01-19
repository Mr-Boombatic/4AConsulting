using System.IO;

namespace _4AConsultingMVC.Services
{
    public static class Logger
    {
        public static void LogError(string message, Exception ex = null)
        {
            try
            {
                string errorMessage = message;
                if (ex != null)
                {
                    errorMessage += $"{Environment.NewLine}Stack trace: {ex.StackTrace}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"{Environment.NewLine}Inner exception: {ex.InnerException.Message}";
                    }
                }

                string logPath = GetLogPath();
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {errorMessage}{Environment.NewLine}";
                File.AppendAllText(logPath, logEntry);
                System.Diagnostics.Debug.WriteLine(errorMessage);
            }
            catch
            {
            }
        }

        private static string GetLogPath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logDirectory = Path.Combine(baseDirectory, "App_Data");
            
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            return Path.Combine(logDirectory, "error.log");
        }
    }
}
