//
// Project: Mark5.Mobile.IOS
// File: SimpleLogger.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.IOS.Utilities
{
    
    public class SimpleLogger : AbstractLogger
    {
        
        protected override void WriteToLog(LogLevel logLevel, string message, Exception exception, bool includeStackTrace = false)
        {
            if (!Enabled || Level < logLevel)
            {
                return;
            }

            // Build message
            var logMessageBuilder = new StringBuilder();
            //logMessageBuilder.Append ("[").Append (DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss.ffff")).Append ("] ");
            logMessageBuilder.Append("[").Append(logLevel).Append("] ");
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
                logMessageBuilder.AppendLine($"{exception.GetType()}: ").AppendLine(exception.Message).Append(" ").Append(exception.StackTrace);
            }

            Console.WriteLine(logMessageBuilder);
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
