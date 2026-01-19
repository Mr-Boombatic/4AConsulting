using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace _4AConsultingWebForms.Services
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
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath("~/App_Data/error.log");
            }
            
            string path = HostingEnvironment.MapPath("~/App_Data/error.log");
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "error.log");
            }
            
            return path;
        }
    }
}
