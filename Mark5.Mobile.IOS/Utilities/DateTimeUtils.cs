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

        static NSDateFormatter timeFormatter;
        static NSDateFormatter dateFormatter;
        static NSDateFormatter longDateFormatter;
        static NSDateFormatter dateTimeFormatter;

        static DateTimeUtils()
        {
            timeFormatter = new NSDateFormatter();
            timeFormatter.DateStyle = NSDateFormatterStyle.None;
            timeFormatter.TimeStyle = NSDateFormatterStyle.Short;
            timeFormatter.TimeZone = new NSTimeZone("GMT");

            dateFormatter = new NSDateFormatter();
            dateFormatter.DateStyle = NSDateFormatterStyle.Short;
            dateFormatter.TimeStyle = NSDateFormatterStyle.None;
            dateFormatter.TimeZone = new NSTimeZone("GMT");

            longDateFormatter = new NSDateFormatter();
            longDateFormatter.DateStyle = NSDateFormatterStyle.Medium;
            longDateFormatter.TimeStyle = NSDateFormatterStyle.None;
            longDateFormatter.TimeZone = new NSTimeZone("GMT");

            dateTimeFormatter = new NSDateFormatter();
            dateTimeFormatter.DateStyle = NSDateFormatterStyle.Short;
            dateTimeFormatter.TimeStyle = NSDateFormatterStyle.Short;
            dateTimeFormatter.TimeZone = new NSTimeZone("GMT");
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
            return timeFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsDateString(this long timestamp)
        {
            return dateFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsLongDateString(this long timestamp)
        {
            return longDateFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        /// IMPORTANT!!!
        /// THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatServerTimestampAsTimeAndDateString(this long timestamp)
        {
            return dateTimeFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
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
