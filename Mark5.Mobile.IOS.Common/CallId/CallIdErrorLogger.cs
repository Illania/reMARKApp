using System;
using Foundation;
using reMARK.Mobile.IOS.Common.Base;

namespace Mark5.Mobile.IOS.Common.CallId
{
    public static class CallIdErrorLogger 
    {
        private static string logPath = NSFileManager.DefaultManager.GetContainerUrl(CallIdContainerUtilities.AppGroupId).Path;
        private static string logFilePath = $"{logPath}/Mark5.Mobile.IOS.Extensions.CallId_{DateTime.Now.ToString("yyyy_M_dd")}.log";

        public static void WriteToLog(string message, Exception ex = null)
        {
            new ErrorLogger(logFilePath).WriteToLog(message, ex);
        }
    }
}
