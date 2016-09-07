//
// Project: Mark5.Mobile.Forms.Droid
// File: SimpleLogger.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2014 Nordic IT
//

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Android.Util;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Utilities
{

    public class SimpleLogger : AbstractLogger
    {

        const string Tag = "MARK5";

        protected override void WriteToLog(LogLevel logLevel, string message, Exception exception, bool includeStackTrace = false)
        {
            if (!Enabled || Level < logLevel)
            {
                return;
            }

            // Build message
            var logMessageBuilder = new StringBuilder();
            //logMessageBuilder.Append ("[").Append (DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss.ffff")).Append ("] ");
            //logMessageBuilder.Append("[").Append(logLevel).Append("] ");
            logMessageBuilder.Append("[").Append(Thread.CurrentThread.ManagedThreadId).Append("] ");
            logMessageBuilder.Append("[").Append(GetStackInfo(3)).Append("] ");
            logMessageBuilder.Append(message);

            if (includeStackTrace)
            {
#pragma warning disable XS0001 // Find usages of mono todo items
                logMessageBuilder.Append("\n Stacktrace: ").Append(new StackTrace(true));
#pragma warning restore XS0001 // Find usages of mono todo items
            }

            if (exception != null)
            {
                logMessageBuilder.Append($"{exception.GetType()}: ").Append(exception.Message).Append(" ").Append(exception.StackTrace);
            }

            LogPriority priority = LogPriority.Info;
            switch (logLevel)
            {
                case LogLevel.TRACE:
                    priority = LogPriority.Verbose;
                    break;
                case LogLevel.DEBUG:
                    priority = LogPriority.Debug;
                    break;
                case LogLevel.INFO:
                    priority = LogPriority.Info;
                    break;
                case LogLevel.WARNING:
                    priority = LogPriority.Warn;
                    break;
                case LogLevel.ERROR:
                    priority = LogPriority.Error;
                    break;
            }

            Log.WriteLine(priority, Tag, logMessageBuilder.ToString());
        }

        static string GetStackInfo(int depth)
        {
#pragma warning disable XS0001 // Find usages of mono todo items
            var sf = new StackFrame(depth, true);
#pragma warning restore XS0001 // Find usages of mono todo items
            return sf.GetMethod().DeclaringType + ":" + sf.GetMethod().Name + ":" + sf.GetFileLineNumber();
        }
    }
}
