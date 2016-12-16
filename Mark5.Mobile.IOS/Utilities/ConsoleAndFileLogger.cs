//
// Project: Mark5.Mobile.IOS
// File: ConsoleAndFileLogger.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Foundation;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.IOS.Utilities
{

    public class ConsoleAndFileLogger : AbstractLogger
    {

        readonly string logPath;
        readonly string logFilePath;
        readonly object logFileLock = new object();

        public ConsoleAndFileLogger()
        {
            try
            {
                NSError err;
                var fm = NSFileManager.DefaultManager;

                var paths = NSSearchPath.GetDirectories(NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User, true);
                logPath = $"{paths[0]}/v2/logs";

                if (!fm.FileExists(logPath))
                {
                    fm.CreateDirectory(logPath, true, new NSFileAttributes(), out err);
                    if (err != null) throw new NSErrorException(err);
                }

                logFilePath = $"{logPath}/mark5_ios_{DateTime.Now.ToString("yyyy_M_dd")}.log";

                if (!fm.FileExists(logFilePath))
                {
                    fm.CreateFile(logFilePath, new NSData(), new NSFileAttributes());
                }

#if DEBUG
                WriteToConsole("Log path: " + logPath);
                WriteToConsole("Log file path: " + logFilePath);
#endif

                WriteToFile("******************************");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        
        protected override void WriteToLog(LogLevel logLevel, string message, Exception exception, bool includeStackTrace = false)
        {
            if (!Enabled || Level < logLevel)
            {
                return;
            }

            // Build message
            var logMessageBuilder = new StringBuilder();
            logMessageBuilder.Append ("[").Append (DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss.ffff")).Append ("] ");
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

            var log = logMessageBuilder.ToString();

            WriteToConsole(log);
            WriteToFile(log);
        }

        void WriteToConsole(string log) => Console.WriteLine(log);

        void WriteToFile(string log)
        {
            lock (logFileLock)
            {
                if (string.IsNullOrWhiteSpace(logFilePath)) return;

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

        public string ReadLogFile()
        {
            lock (logFileLock)
            {
                if (string.IsNullOrWhiteSpace(logFilePath)) return string.Empty;

                try
                {
                    using (var file = NSFileHandle.OpenRead(logFilePath))
                    {
                        var data = file.ReadDataToEndOfFile();
                        return NSString.FromData(data, NSStringEncoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return string.Empty;
                }
            }
        }

        public bool CleanUpOldLogFiles()
        {
            lock (logFileLock)
            {
                if (string.IsNullOrWhiteSpace(logPath)) return false;

                try
                {
                    NSError err;
                    var fm = NSFileManager.DefaultManager;

                    var contents = fm.GetDirectoryContent(logPath, out err);
                    if (err != null) throw new NSErrorException(err);

                    var sorted = contents.Where(s => s.StartsWith("mark5_ios_", StringComparison.CurrentCulture)).OrderBy(s => s).ToArray();
                    for (var i = 0; i < sorted.Length - 3; i++)
                    {
                        fm.Remove(logPath + "/" + sorted[i], out err);
                        if (err != null) throw new NSErrorException(err);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
            }
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
