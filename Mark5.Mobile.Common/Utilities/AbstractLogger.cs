using System;

namespace Mark5.Mobile.Common.Utilities
{
    public abstract class AbstractLogger : ILogger
    {
        public bool Enabled { get; set; } = true;

        public LogLevel Level { get; set; }

        #region Trace

        public bool IsTraceEnabled()
        {
            return Level >= LogLevel.TRACE;
        }

        public void Trace(string message, bool includeStackTrace = false)
        {
            WriteToLog(LogLevel.TRACE, message, null, includeStackTrace);
        }

        public void Trace(string message, Exception exception)
        {
            WriteToLog(LogLevel.TRACE, message, exception);
        }

        #endregion

        #region Debug

        public bool IsDebugEnabled()
        {
            return Level >= LogLevel.DEBUG;
        }

        public void Debug(string message)
        {
            WriteToLog(LogLevel.DEBUG, message, null);
        }

        public void Debug(string message, Exception exception)
        {
            WriteToLog(LogLevel.DEBUG, message, exception);
        }

        #endregion

        #region Info

        public bool IsInfoEnabled()
        {
            return Level >= LogLevel.INFO;
        }

        public void Info(string message)
        {
            WriteToLog(LogLevel.INFO, message, null);
        }

        public void Info(string message, Exception exception)
        {
            WriteToLog(LogLevel.INFO, message, exception);
        }

        #endregion

        #region Warning

        public bool IsWarningEnabled()
        {
            return Level >= LogLevel.WARNING;
        }

        public void Warning(string message)
        {
            WriteToLog(LogLevel.WARNING, message, null);
        }

        public void Warning(string message, Exception exception)
        {
            WriteToLog(LogLevel.WARNING, message, exception);
        }

        #endregion

        #region Error

        public bool IsErrorEnabled()
        {
            return Level >= LogLevel.ERROR;
        }

        public void Error(string message)
        {
            WriteToLog(LogLevel.ERROR, message, null);
        }

        public void Error(Exception exception)
        {
            WriteToLog(LogLevel.ERROR, string.Empty, exception);
        }

        public void Error(string message, Exception exception)
        {
            WriteToLog(LogLevel.ERROR, message, exception);
        }

        #endregion

        protected abstract void WriteToLog(LogLevel logLevel, string message, Exception exception, bool includeStackTrace = false);
    }
}