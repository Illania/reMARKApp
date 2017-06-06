using System;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public static class Formatters
    {
        static readonly string[] SizeSuffixes =
        {
            "B",
            "KB",
            "MB",
            "GB",
            "TB",
            "PB",
            "EB",
            "ZB",
            "YB"
        };

        public static string FormatFileSize(long bytes)
        {
            try
            {
                if (bytes < 0)
                    return "Unknown size";

                var mag = (int) Math.Log(bytes, 1024);
                decimal adjustedSize = (decimal) bytes / (1L << (mag * 10));

                return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to get size for {bytes} bytes.", ex);

                return "Unknown size";
            }
        }
    }
}