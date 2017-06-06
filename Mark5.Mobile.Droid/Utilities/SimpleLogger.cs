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
                return;

            var p = LogPriority.Info;
            switch (logLevel)
            {
                case LogLevel.TRACE:
                    p = LogPriority.Verbose;
                    break;
                case LogLevel.DEBUG:
                    p = LogPriority.Debug;
                    break;
                case LogLevel.INFO:
                    p = LogPriority.Info;
                    break;
                case LogLevel.WARNING:
                    p = LogPriority.Warn;
                    break;
                case LogLevel.ERROR:
                    p = LogPriority.Error;
                    break;
            }

            // Build message
            Write(p, "[" + Thread.CurrentThread.ManagedThreadId + "] [" + GetStackInfo(3) + "] " + message);

            if (includeStackTrace)
            {
#pragma warning disable XS0001 // Find usages of mono todo items
                Write(p, "Stacktrace:");
                Write(p, new StackTrace(true).ToString());
#pragma warning restore XS0001 // Find usages of mono todo items
            }

            if (exception != null)
            {
                Write(p, "Exception: " + exception.GetType() + ": " + exception.Message);
                Write(p, "Exception stacktrace:");
                Write(p, exception.StackTrace ?? "<no stacktrace available>");
            }
        }

        void Write(LogPriority p, string s) => Log.WriteLine(p, Tag, s ?? "");

        static string GetStackInfo(int depth)
        {
#pragma warning disable XS0001 // Find usages of mono todo items
            var sf = new StackFrame(depth, true);
#pragma warning restore XS0001 // Find usages of mono todo items
            return sf.GetMethod().DeclaringType + ":" + sf.GetMethod().Name + ":" + sf.GetFileLineNumber();
        }
    }
}