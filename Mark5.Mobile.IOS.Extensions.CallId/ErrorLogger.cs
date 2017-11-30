using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Foundation;

namespace CallOverlayExtension
{
    public class ErrorLogger
    {
        readonly string logPath;
        readonly string logFilePath;
        const string appGroupId = "group.com.nordic-it.mark5.mobile.ios";

        public ErrorLogger()
        {
            try
            {
                NSError err;
                var fm = NSFileManager.DefaultManager;

                var logPath = NSFileManager.DefaultManager.GetContainerUrl(appGroupId).Path;

                logFilePath = $"{logPath}/calloverlayextension_{DateTime.Now.ToString("yyyy_M_dd")}.log";

                if (!fm.FileExists(logFilePath))
                    fm.CreateFile(logFilePath, new NSData(), new NSFileAttributes());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void WriteToLog(Exception exception)
        {
            // Build message
            var logMessageBuilder = new StringBuilder();
            logMessageBuilder.Append("[").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")).Append("] ");

            logMessageBuilder.AppendLine("Stacktrace: ").Append(new StackTrace(true));

            if (exception != null)
                logMessageBuilder.AppendLine($"{exception.GetType()}: ").AppendLine(exception.Message).Append(" ").Append(exception.StackTrace);

            var log = logMessageBuilder.ToString();
        }
    }
}
