using log4net;
using log4net.Config;
using PolicyTracker.Platform.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyTracker.Platform.Logging
{
    public enum LogLevel { FATAL, ERROR, WARN, INFO, DEBUG }

    public static class LogManager
    {
        private static readonly ILog _Logger = log4net.LogManager.GetLogger("PolicyTracker");
        private static readonly ILog _PerformanceLogger = log4net.LogManager.GetLogger("PolicyTracker.Performance");

        public static bool IsLogLevelEnabled(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.FATAL:
                    return _Logger.IsFatalEnabled;
                case LogLevel.ERROR:
                    return _Logger.IsErrorEnabled;
                case LogLevel.WARN:
                    return _Logger.IsWarnEnabled;
                case LogLevel.INFO:
                    return _Logger.IsInfoEnabled;
                case LogLevel.DEBUG:
                    return _Logger.IsDebugEnabled;
            }

            return false;
        }

        public static void Log(LogLevel level, string message)
        {
            //var user = SessionManager.GetCurrentSession().User;
            //message = "[User: " + user.UserName + "] " + message;
            Log(_Logger, level, message);
        }

        public static void Log(LogLevel level, string formattedMessage, object param0, object param1 = null, object param2 = null)
        {
            if (param2 != null)
            {
                Log(level, String.Format(formattedMessage, param0, param1, param2));
            }
            else if (param1 != null)
            {
                Log(level, String.Format(formattedMessage, param0, param1));
            }
            else
            {
                Log(level, String.Format(formattedMessage, param0));
            }
        }

        public static void Log(LogLevel level, string formattedMessage, object[] paramz)
        {
            Log(level, String.Format(formattedMessage, paramz));
        }

        public static void LogPerformance(LogLevel level, string message)
        {
            Log(_PerformanceLogger, level, message);
        }

        public static void LogPerformance(LogLevel level, long elapsedTime, string message)
        {
            var logString = new StringBuilder("[").Append(elapsedTime).Append("ms]  ").Append(message).ToString();
            LogPerformance(level, logString);
        }

        private static void Log(ILog logger, LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.FATAL:
                    logger.Fatal(message);
                    break;
                case LogLevel.ERROR:
                    logger.Error(message);
                    break;
                case LogLevel.WARN:
                    logger.Warn(message);
                    break;
                case LogLevel.INFO:
                    logger.Info(message);
                    break;
                case LogLevel.DEBUG:
                    logger.Debug(message);
                    break;
            }
        }

        public static void Initialize()
        {
            //XmlConfigurator.Configure();
        }
    }
}
