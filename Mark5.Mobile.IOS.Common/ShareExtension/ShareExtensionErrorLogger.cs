using System;
using Foundation;
using reMARK.Mobile.IOS.Common.Base;

namespace Mark5.Mobile.IOS.Common.ShareExtension
{
    public static class ShareExtensionErrorLogger 
    {
        private static string logPath = NSFileManager.DefaultManager.GetContainerUrl(ShareExtensionContainerUtilities.AppGroupId).Path;
        private static string logFilePath = $"{logPath}/Mark5.Mobile.IOS.Extensions.Share_{DateTime.Now:yyyy_M_dd}.log";

        public static void WriteToLog(string message, Exception ex = null)
        {
            new ErrorLogger(logFilePath).WriteToLog(message, ex);
        }
    }
}
