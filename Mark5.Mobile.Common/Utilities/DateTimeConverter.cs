using System;

namespace Mark5.Mobile.Common.Utilities
{
    public static class DateTimeConverter
    {
        public static bool UseServerTimezone = true;

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
            return (long)elapsed.TotalMilliseconds;
        }

        public static DateTime ConvertUtcToUserTime(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            return dt.AddHours(UseServerTimezone ? +GetServerUtcOffset(dateTime) : +GetLocalUtcOffset(dateTime));
        }

        public static DateTime ConvertUserTimeToUtc(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dt.AddHours(UseServerTimezone ? -GetServerUtcOffset(dateTime) : -GetLocalUtcOffset(dateTime));
        }

        public static DateTime ConvertUtcToServerTime(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            return dt.AddHours(GetServerUtcOffset(dateTime));
        }

        public static DateTime ConvertServerTimeToUtc(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dt.AddHours(-GetServerUtcOffset(dateTime));
        }

        static double GetLocalUtcOffset(DateTime dateTime) => TimeZoneInfo.Local.GetUtcOffset(dateTime).TotalHours;

        static double GetServerUtcOffset(DateTime dateTime)
        {
            double offset;

            var serverTimeZoneInfo = ServerConfig.SystemSettings.SystemInfo.ServerTimeZoneInfo.Value;

            if (serverTimeZoneInfo != null)
            {
                try
                {
                    offset = serverTimeZoneInfo.GetUtcOffset(dateTime).TotalHours;
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while trying to get server UTC offset", ex);
                    offset = ServerConfig.SystemSettings.SystemInfo.ServerUtcOffset.TotalHours;
                }
            }
            else
                offset = ServerConfig.SystemSettings.SystemInfo.ServerUtcOffset.TotalHours;

            return offset;
        }

    }
}