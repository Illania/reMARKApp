//
// Project: Mark5.Mobile.IOS
// File: DateTimeUtils.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Foundation;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Utilities
{

    public static class DateTimeUtils
    {

        public static NSDateFormatter TimeFormatter;
        public static NSDateFormatter DateFormatter;
        public static NSDateFormatter LongDateFormatter;
        public static NSDateFormatter DateTimeFormatter;

        static DateTimeUtils()
        {
            TimeFormatter = new NSDateFormatter();
            TimeFormatter.DateStyle = NSDateFormatterStyle.None;
            TimeFormatter.TimeStyle = NSDateFormatterStyle.Short;
            TimeFormatter.TimeZone = new NSTimeZone("GMT");

            DateFormatter = new NSDateFormatter();
            DateFormatter.DateStyle = NSDateFormatterStyle.Short;
            DateFormatter.TimeStyle = NSDateFormatterStyle.None;
            DateFormatter.TimeZone = new NSTimeZone("GMT");

            LongDateFormatter = new NSDateFormatter();
            LongDateFormatter.DateStyle = NSDateFormatterStyle.Medium;
            LongDateFormatter.TimeStyle = NSDateFormatterStyle.None;
            LongDateFormatter.TimeZone = new NSTimeZone("GMT");

            DateTimeFormatter = new NSDateFormatter();
            DateTimeFormatter.DateStyle = NSDateFormatterStyle.Short;
            DateTimeFormatter.TimeStyle = NSDateFormatterStyle.Short;
            DateTimeFormatter.TimeZone = new NSTimeZone("GMT");
        }

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

        public static string FormatServerTimestampAsTimeString(this long timestamp)
        {
            return TimeFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsDateString(this long timestamp)
        {
            return DateFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsLongDateString(this long timestamp)
        {
            return LongDateFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsTimeAndDateString(this long timestamp)
        {
            return DateTimeFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsCompactShortDateTimeString(this long timestamp)
        {
            var serverTimezone = timestamp.ConvertTimestampMillisecondsToDateTime();
            var nowUtc = DateTime.UtcNow.ConvertUtcToServerTime();

            if (serverTimezone.Date == nowUtc.Date)
            {
                return timestamp.FormatServerTimestampAsTimeString();
            }
            if (serverTimezone.Date == nowUtc.Date.AddDays(-1))
            {
                return Localization.GetString("yesterday");
            }

            return timestamp.FormatServerTimestampAsDateString();
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsCompactLongDateTimeString(this long timestamp)
        {
            var serverTimezone = timestamp.ConvertTimestampMillisecondsToDateTime();
            var nowUtc = DateTime.UtcNow.ConvertUtcToServerTime();

            if (serverTimezone.Date == nowUtc.Date)
            {
                return timestamp.FormatServerTimestampAsTimeString() + ", " + Localization.GetString("today");
            }
            if (serverTimezone.Date == nowUtc.Date.AddDays(-1))
            {
                return timestamp.FormatServerTimestampAsTimeString() + ", " + Localization.GetString("yesterday");
            }

            return timestamp.FormatServerTimestampAsTimeAndDateString();
        }
    }
}
