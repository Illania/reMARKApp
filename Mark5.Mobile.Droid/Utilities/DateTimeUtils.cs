using System;
using Android.Content;
using Android.Text.Format;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class DateTimeUtils
    {
        public static bool UseServerTimezone = true;

        static readonly int LocalUtcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;

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

        public static string FormatUserTimestampAsTimeString(this long timestamp, Context context)
        {
            var date = new Java.Util.Date(timestamp);
            var tf = DateFormat.GetTimeFormat(context);
            tf.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            return tf.Format(date);
        }

        public static string FormatUserTimestampAsDateString(this long timestamp, Context context)
        {
            var date = new Java.Util.Date(timestamp);
            var df = DateFormat.GetDateFormat(context);
            df.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            return df.Format(date);
        }

        public static string FormatUserTimestampAsLongDateString(this long timestamp, Context context)
        {
            var date = new Java.Util.Date(timestamp);
            var df = DateFormat.GetMediumDateFormat(context);
            df.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            return df.Format(date);
        }

        public static string FormatUserTimestampAsTimeAndDateString(this long timestamp, Context context)
        {
            var date = new Java.Util.Date(timestamp);
            var tf = DateFormat.GetTimeFormat(context);
            tf.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            var df = DateFormat.GetDateFormat(context);
            df.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            return tf.Format(date) + ", " + df.Format(date);
        }

        public static string FormatUserTimestampAsCompactShortDateTimeString(this long timestamp, Context context)
        {
            var serverTimezone = timestamp.ConvertTimestampMillisecondsToDateTime();
            var nowUtc = DateTime.UtcNow.ConvertUtcToUserTime();

            if (serverTimezone.Date == nowUtc.Date)
                return timestamp.FormatUserTimestampAsTimeString(context);
            if (serverTimezone.Date == nowUtc.Date.AddDays(-1))
                return context.GetString(Resource.String.yesterday);

            return timestamp.FormatUserTimestampAsDateString(context);
        }

        public static string FormatUserTimestampAsCompactLongDateTimeString(this long timestamp, Context context)
        {
            var serverTimezone = timestamp.ConvertTimestampMillisecondsToDateTime();
            var nowUtc = DateTime.UtcNow.ConvertUtcToUserTime();

            if (serverTimezone.Date == nowUtc.Date)
                return timestamp.FormatUserTimestampAsTimeString(context) + ", " + context.GetString(Resource.String.today);
            if (serverTimezone.Date == nowUtc.Date.AddDays(-1))
                return timestamp.FormatUserTimestampAsTimeString(context) + ", " + context.GetString(Resource.String.yesterday);

            return timestamp.FormatUserTimestampAsTimeAndDateString(context);
        }
    }
}