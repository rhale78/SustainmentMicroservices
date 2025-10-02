using System.Diagnostics;

namespace Log
{
    public class LogInstance
    {
        public static LogInstance CreateLog()
        {
            return new LogInstance();
        }
        public void LogInformation(string message, params object[] args)
        {

        }
        public void LogWarning(string message, params object[] args)
        {

        }
        public void LogError(string message, params object[] args)
        {

        }
        public void LogDebug(string message, params object[] args)
        {
            Debug.WriteLine(message + "    \"" + string.Join(",\"", args) + "\"");
        }
        public void LogTrace(string message, params object[] args)
        {

        }
        public void LogCritical(string message, params object[] args)
        {

        }
        public void LogException(Exception ex)
        {

        }
    }
}
