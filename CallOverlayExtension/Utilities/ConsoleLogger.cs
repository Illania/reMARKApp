using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Mark5.Mobile.Common.Utilities;

namespace CallOverlayExtension.Utilities
{
    public class ConsoleLogger : AbstractLogger
    {
        protected override void WriteToLog(LogLevel logLevel, string message, Exception exception, bool includeStackTrace = false)
        {
            if (!Enabled || Level < logLevel)
                return;

            // Build message
            var logMessageBuilder = new StringBuilder();
            logMessageBuilder.Append("[").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")).Append("] ");
            logMessageBuilder.Append("[").Append(logLevel).Append("] ");
            logMessageBuilder.Append("[").Append(Thread.CurrentThread.ManagedThreadId).Append("] ");
            logMessageBuilder.Append("[").Append(GetStackInfo(3)).Append("] ");
            logMessageBuilder.Append(message).Append(" ");

            if (includeStackTrace)
                logMessageBuilder.AppendLine("Stacktrace: ").Append(new StackTrace(true));

            if (exception != null)
                logMessageBuilder.AppendLine($"{exception.GetType()}: ").AppendLine(exception.Message).Append(" ").Append(exception.StackTrace);

            var log = logMessageBuilder.ToString();

            WriteToConsole(log);
        }

        void WriteToConsole(string log)
        {
            Console.WriteLine(log);
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