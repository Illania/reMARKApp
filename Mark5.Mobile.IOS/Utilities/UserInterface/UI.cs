//
// Project: Mark5.Mobile.IOS
// File: UI.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
namespace Mark5.Mobile.IOS.Utilities.UserInterface
{
    public static class UI
    {
        #region Pretty printing

        static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string PrettyFileSize(long bytes)
        {
            var mag = (int)Math.Log(bytes, 1024);
            decimal adjustedSize = (decimal)bytes / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }

        #endregion
    }
}
