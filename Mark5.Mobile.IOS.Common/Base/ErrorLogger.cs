using System;
using System.Diagnostics;
using System.Text;
using Foundation;

namespace reMARK.Mobile.IOS.Common.Base
{
    public class ErrorLogger
    {
        readonly object logFileLock = new();
        readonly string logFilePath;

        public ErrorLogger(string logFilePath)
        {
            var fm = NSFileManager.DefaultManager;

            this.logFilePath = logFilePath;

            if (!fm.FileExists(logFilePath))
                fm.CreateFile(logFilePath, new NSData(), new NSFileAttributes());

        }

        public void WriteToLog(string message, Exception exception = null)
        {
            var logMessageBuilder = new StringBuilder();
            logMessageBuilder.Append("[").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")).Append("] ");
            logMessageBuilder.Append(message).Append(" ");

            logMessageBuilder.AppendLine("Stacktrace: ").Append(new StackTrace(true));

            if (exception != null)
                logMessageBuilder.AppendLine($"{exception.GetType()}: ").AppendLine(exception.Message).Append(" ").Append(exception.StackTrace);

            var log = logMessageBuilder.ToString();

            WriteToFile(log);

        }

        private void WriteToFile(string log)
        {
            lock (logFileLock)
            {
                if (string.IsNullOrWhiteSpace(logFilePath))
                    return;

                try
                {
                    using (var file = NSFileHandle.OpenWrite(logFilePath))
                    {
                        file.SeekToEndOfFile();
                        file.WriteData(NSData.FromString("\n", NSStringEncoding.UTF8));
                        file.WriteData(NSData.FromString(log, NSStringEncoding.UTF8));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        
    }
}
