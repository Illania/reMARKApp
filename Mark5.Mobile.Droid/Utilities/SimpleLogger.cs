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
using Xamarin;

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
            logMessageBuilder.Append(" [").Append(logLevel).Append("] ");
            logMessageBuilder.Append(" [").Append(Thread.CurrentThread.ManagedThreadId).Append("] ");
            logMessageBuilder.Append(" [").Append(GetStackInfo(4)).Append("] ");
            logMessageBuilder.Append(message);

            if (includeStackTrace)
            {
                logMessageBuilder.Append("\n Stacktrace: ").Append(new StackTrace(true));
            }

            if (exception != null)
            {
                logMessageBuilder.Append(" Exception: ").Append(exception.Message).Append(" ").Append(exception.StackTrace);
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

            LogToInsightsIfNecessary(logLevel, exception);
        }

        static void LogToInsightsIfNecessary(LogLevel logLevel, Exception exception)
        {
            try
            {
                if (exception != null)
                {
                    if (logLevel == LogLevel.ERROR)
                    {
                        Insights.Report(exception, Insights.Severity.Error);
                    }
                    else
                    {
                        Insights.Report(exception);
                    }
                }
            }
            catch (Exception e)
            {
                // Let's catch this exception in case Insights are failing,
                // so we do not crash the entire application.
                Log.WriteLine(LogPriority.Error, Tag, "Error occured in Xamarin Insights: " + e.Message);
            }
        }

        static string GetStackInfo(int depth)
        {
            var sf = new StackFrame(depth, true);
            return sf.GetMethod().DeclaringType + ":" + sf.GetMethod().Name + ":" + sf.GetFileLineNumber();
        }
    }
}
