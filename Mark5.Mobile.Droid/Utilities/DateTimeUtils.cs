//
// Project: Mark5.Mobile.Droid
// File: DateTimeUtils.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

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
    }
}
