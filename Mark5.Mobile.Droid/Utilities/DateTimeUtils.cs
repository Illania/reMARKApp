//
// Project: Mark5.Mobile.Droid
// File: DateTimeUtils.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.Text.Format;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Utilities
{

    public static class DateTimeUtils
    {

        public static DateTime ConvertUtcToServerTime(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            return dt.AddHours(ServerConfig.SystemSettings.SystemInfo.ServerUtcOffset.Hours);
        }

        public static DateTime ConvertServerTimeToUtc(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dt.AddHours(-ServerConfig.SystemSettings.SystemInfo.ServerUtcOffset.Hours);
        }

        public static string FormatServerTimestampAsTimeString(this long timestamp, Context context)
        {
            var date = new Java.Util.Date(timestamp);
            var tf = DateFormat.GetTimeFormat(context);
            tf.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            return tf.Format(date);
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsDateString(this long timestamp, Context context)
        {
            var date = new Java.Util.Date(timestamp);
            var df = DateFormat.GetDateFormat(context);
            df.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            return df.Format(date);
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsTimeAndDateString(this long timestamp, Context context)
        {
            var date = new Java.Util.Date(timestamp);
            var tf = DateFormat.GetTimeFormat(context);
            tf.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            var df = DateFormat.GetDateFormat(context);
            df.TimeZone = Java.Util.TimeZone.GetTimeZone("GMT");
            return tf.Format(date) + ", " + df.Format(date);
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsCompactShortDateTimeString(this long timestamp, Context context)
        {
            var serverTimestamp = timestamp.ConvertTimestampMillisecondsToDateTime().ConvertServerTimeToUtc();
            var nowUtc = DateTime.UtcNow;

            if (serverTimestamp.Date == nowUtc.Date)
            {
                return timestamp.FormatServerTimestampAsTimeString(context);
            }
            if (serverTimestamp.Date == nowUtc.Date.AddDays(-1))
            {
                return context.GetString(Resource.String.yesterday);
            }

            return timestamp.FormatServerTimestampAsDateString(context);
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsCompactLongDateTimeString(this long timestamp, Context context)
        {
            var serverTimestamp = timestamp.ConvertTimestampMillisecondsToDateTime().ConvertServerTimeToUtc();
            var nowUtc = DateTime.UtcNow;

            if (serverTimestamp.Date == nowUtc.Date)
            {
                return timestamp.FormatServerTimestampAsTimeString(context) + ", " + context.GetString(Resource.String.today);
            }
            if (serverTimestamp.Date == nowUtc.Date.AddDays(-1))
            {
                return timestamp.FormatServerTimestampAsTimeString(context) + ", " + context.GetString(Resource.String.yesterday);
            }

            return timestamp.FormatServerTimestampAsTimeAndDateString(context);
        }
    }
}
