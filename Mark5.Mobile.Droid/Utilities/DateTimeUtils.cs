//
// Project: Mark5.Mobile.Droid
// File: DateTimeUtils.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Java.Lang;

namespace Mark5.Mobile.Droid.Utilities
{

    public static class DateTimeUtils
    {

        public static DateTime ToServerTime(this DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            dt = dt.AddHours(ServerConfig.SystemSettings.SystemInfo.ServerUtcOffset.Hours);
            return dt;
        }

        public static DateTime FromJavaTimeStamp(this long timestamp)
        {
            var dto = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
            return dto.UtcDateTime;
        }

        public static long ToJavaTimeStamp(this DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc && dateTime.Kind != DateTimeKind.Unspecified)
            {
                throw new IllegalArgumentException($"Illegal {nameof(dateTime)}.{nameof(DateTime.Kind)}!");
            }

            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }
    }
}
