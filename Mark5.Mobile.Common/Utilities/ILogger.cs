//
// File: ILogger.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2014 Nordic IT
//

using System;

namespace Mark5.Mobile.Common.Utilities
{
    public enum LogLevel
    {
        TRACE = 5,
        DEBUG = 4,
        INFO = 3,
        WARNING = 2,
        ERROR = 1,
        OFF = 0,
    }

    public interface ILogger
    {
        LogLevel Level { get; set; }

        #region Trace

        bool IsTraceEnabled();

        void Trace(string message, bool includeStackTrace = false);

        void Trace(string message, Exception exception);

        #endregion

        #region Debug

        bool IsDebugEnabled();

        void Debug(string message);

        void Debug(string message, Exception exception);

        #endregion

        #region Info

        bool IsInfoEnabled();

        void Info(string message);

        void Info(string message, Exception exception);

        #endregion

        #region Warning

        bool IsWarningEnabled();

        void Warning(string message);

        void Warning(string message, Exception exception);

        #endregion

        #region Error

        bool IsErrorEnabled();

        void Error(string message);

        void Error(Exception exception);

        void Error(string message, Exception exception);

        #endregion
    }
}