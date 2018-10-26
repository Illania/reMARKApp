using System;
using Foundation;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class DateTimeFormatter
    {
        public static NSDateFormatter ShortTimeFormatter;
        public static NSDateFormatter ShortDateFormatter;
        public static NSDateFormatter ShortDateTimeFormatter;
        public static NSDateFormatter LongDateFormatter;

        static DateTimeFormatter()
        {
            ShortTimeFormatter = new NSDateFormatter()
            {
                DateStyle = NSDateFormatterStyle.None,
                TimeStyle = NSDateFormatterStyle.Short,
                TimeZone = new NSTimeZone("GMT")
            };

            ShortDateFormatter = new NSDateFormatter()
            {
                DateStyle = NSDateFormatterStyle.Short,
                TimeStyle = NSDateFormatterStyle.None,
                TimeZone = new NSTimeZone("GMT")
            };

            ShortDateTimeFormatter = new NSDateFormatter()
            {
                DateStyle = NSDateFormatterStyle.Short,
                TimeStyle = NSDateFormatterStyle.Short,
                TimeZone = new NSTimeZone("GMT")
            };

            LongDateFormatter = new NSDateFormatter()
            {
                DateStyle = NSDateFormatterStyle.Medium,
                TimeStyle = NSDateFormatterStyle.None,
                TimeZone = new NSTimeZone("GMT")
            };
        }

        public static string FormatUserTimestampAsTimeString(this long timestamp)
        {
            return ShortTimeFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        ///     IMPORTANT!!!
        ///     THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatUserTimestampAsDateString(this long timestamp)
        {
            return ShortDateFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        ///     IMPORTANT!!!
        ///     THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatUserTimestampAsLongDateString(this long timestamp)
        {
            return LongDateFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        ///     IMPORTANT!!!
        ///     THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatUserTimestampAsTimeAndDateString(this long timestamp)
        {
            return ShortDateTimeFormatter.StringFor(NSDate.FromTimeIntervalSince1970(timestamp / 1000));
        }

        /// <summary>
        ///     IMPORTANT!!!
        ///     THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatUserTimestampAsCompactShortDateTimeString(this long timestamp)
        {
            var serverTimezone = timestamp.ConvertTimestampMillisecondsToDateTime();
            var nowUtc = DateTime.UtcNow.ConvertUtcToUserTime();

            if (serverTimezone.Date == nowUtc.Date)
                return timestamp.FormatUserTimestampAsTimeString();
            if (serverTimezone.Date == nowUtc.Date.AddDays(-1))
                return Localization.GetString("yesterday");

            return timestamp.FormatUserTimestampAsDateString();
        }

        /// <summary>
        ///     IMPORTANT!!!
        ///     THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatUserTimestampAsCompactMediumDateTimeString(this long timestamp)
        {
            var serverTimezone = timestamp.ConvertTimestampMillisecondsToDateTime();
            var nowUtc = DateTime.UtcNow.ConvertUtcToUserTime();

            if (serverTimezone.Date == nowUtc.Date)
                return timestamp.FormatUserTimestampAsTimeString();

            return timestamp.FormatUserTimestampAsTimeAndDateString();
        }

        /// <summary>
        ///     IMPORTANT!!!
        ///     THIS METHOD ACCEPTS TIMESTAMP IN SERVER TIMEZONE
        /// </summary>
        public static string FormatUserTimestampAsCompactLongDateTimeString(this long timestamp)
        {
            var serverTimezone = timestamp.ConvertTimestampMillisecondsToDateTime();
            var nowUtc = DateTime.UtcNow.ConvertUtcToUserTime();

            if (serverTimezone.Date == nowUtc.Date)
                return timestamp.FormatUserTimestampAsTimeString() + ", " + Localization.GetString("today");
            if (serverTimezone.Date == nowUtc.Date.AddDays(-1))
                return timestamp.FormatUserTimestampAsTimeString() + ", " + Localization.GetString("yesterday");

            return timestamp.FormatUserTimestampAsTimeAndDateString();
        }
    }
}