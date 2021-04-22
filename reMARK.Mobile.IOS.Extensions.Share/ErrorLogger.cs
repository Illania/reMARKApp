using System;
using Foundation;

namespace reMARK.Mobile.IOS.Extensions
{
    public class ErrorLogger
    {
        public ErrorLogger()
        {
            readonly object logFileLock = new object();
            readonly string logPath;
            readonly string logFilePath;

            public ErrorLogger(string logFilePath)
            {
                var fm = NSFileManager.DefaultManager;

                if (!fm.FileExists(logFilePath))
                    fm.CreateFile(logFilePath, new NSData(), new NSFileAttributes());
            }

            public void WriteToLog(Exception exception)
            {
                var logMessageBuilder = new StringBuilder();
                logMessageBuilder.Append("[").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")).Append("] ");

                logMessageBuilder.AppendLine("Stacktrace: ").Append(new StackTrace(true));

                if (exception != null)
                    logMessageBuilder.AppendLine($"{exception.GetType()}: ").AppendLine(exception.Message).Append(" ").Append(exception.StackTrace);

                var log = logMessageBuilder.ToString();

                WriteToFile(log);
            }

            void WriteToFile(string log)
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
}
