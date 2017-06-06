//
// File: DateTimeUtilities.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

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
            if (dateTime.Kind != DateTimeKind.Utc && dateTime.Kind != DateTimeKind.Unspecified)
            {
                throw new ArgumentException($"Invalid {nameof(dateTime)}.{nameof(DateTime.Kind)}!");
            }
            if (dateTime == default(DateTime))
            {
                return -1;
            }
            var elapsed = dateTime - epoch;
            return (long) elapsed.TotalMilliseconds;
        }
    }
}