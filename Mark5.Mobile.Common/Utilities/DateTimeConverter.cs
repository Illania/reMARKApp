using System;

namespace Mark5.Mobile.Common.Utilities
{
    public static class DateTimeConverter
    {
        public static long ServerDefaultTimestamp = -6847804800000;

        public static bool UseServerTimezone = true;

        static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        static readonly int LocalUtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours;

        public static DateTime ConvertTimestampMillisecondsToDateTime(this long timestamp)
        {
            return epoch.AddMilliseconds(timestamp);
        }

        public static long ConvertDateTimeToTimestampMilliseconds(this DateTime dateTime)
        {
            if (dateTime == default(DateTime))
                return -1;

            var elapsed = dateTime - epoch;
            return (long)elapsed.TotalMilliseconds;
        }

        public static DateTime ConvertUtcToUserTime(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            return dt.AddHours(UseServerTimezone ? ServerConfig.SystemSettings.SystemInfo.ServerUtcOffset.Hours : LocalUtcOffset);
        }

        public static DateTime ConvertUserTimeToUtc(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dt.AddHours(UseServerTimezone ? -ServerConfig.SystemSettings.SystemInfo.ServerUtcOffset.Hours : -LocalUtcOffset);
        }
    }
}