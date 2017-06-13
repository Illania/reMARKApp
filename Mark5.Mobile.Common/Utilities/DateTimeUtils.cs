using System;

namespace Mark5.Mobile.Common.Utilities
{
    public static class DateTimeUtils
    {
        static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        public static DateTime ConvertTimestampMillisecondsToDateTime(this long timestamp)
        {
            return epoch.AddMilliseconds(timestamp);
        }

        public static long ConvertDateTimeToTimestampMilliseconds(this DateTime dateTime)
        {
            if (dateTime == default(DateTime))
                return -1;

            var elapsed = dateTime - epoch;
            return (long) elapsed.TotalMilliseconds;
        }
    }
}