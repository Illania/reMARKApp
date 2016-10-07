//
// Project: Mark5.Mobile.Droid
// File: Formatters.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public static class Formatters
    {

        static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string FormatFileSize(long bytes)
        {
            int mag = (int)Math.Log(bytes, 1024);
            decimal adjustedSize = (decimal)bytes / (1L << (mag * 10));
            return $"{adjustedSize:n1} {SizeSuffixes[mag]}";
        }
    }
}
