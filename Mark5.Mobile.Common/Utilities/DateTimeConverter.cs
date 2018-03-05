using System;

namespace Mark5.Mobile.Common.Utilities
{
    public static class DateTimeConverter
    {
        public static bool UseServerTimezone = true;
        public static Func<string, TimeZoneInfo> GetTimeZoneInfoFromSerializedString;

        static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        static readonly int LocalUtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours;

        static int? serverUtcOffset;
        static int ServerUtcOffset
        {
            get
            {
                if (serverUtcOffset == null)
                {
                    serverUtcOffset = GetServerUtcOffset();
                }
                return serverUtcOffset.Value;
            }
        }

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
            return dt.AddHours(UseServerTimezone ? ServerUtcOffset : LocalUtcOffset);
        }

        public static DateTime ConvertUserTimeToUtc(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dt.AddHours(UseServerTimezone ? -ServerUtcOffset : -LocalUtcOffset);
        }

        public static DateTime ConvertUtcToServerTime(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            return dt.AddHours(ServerUtcOffset);
        }

        public static DateTime ConvertServerTimeToUtc(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dt.AddHours(-ServerUtcOffset);
        }

        static int GetServerUtcOffset()
        {
            int offset;

            if (!string.IsNullOrEmpty(ServerConfig.SystemSettings.SystemInfo.ServerTimeZoneInfoSerialized) && GetTimeZoneInfoFromSerializedString != null)
            {
                try
                {
                    var serverTimeZoneInfo = GetTimeZoneInfoFromSerializedString(ServerConfig.SystemSettings.SystemInfo.ServerTimeZoneInfoSerialized);
                    offset = serverTimeZoneInfo.GetUtcOffset(DateTime.Now).Hours;
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while trying to get server UTC offset", ex);
                    offset = ServerConfig.SystemSettings.SystemInfo.ServerUtcOffset.Hours;
                }
            }
            else
                offset = ServerConfig.SystemSettings.SystemInfo.ServerUtcOffset.Hours;

            return offset;
        }
    }
}