//
// Project: Mark5.Mobile.Common
// File: StringExtensions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;

namespace Mark5.Mobile.Common.Utilities
{

    public static class StringExtensions
    {

        public static string SafeSubstring(this string str, int startIndex)
        {
            if (str.Length <= startIndex)
            {
                return str;
            }

            return str.Substring(startIndex);
        }

        public static string SafeSubstring(this string str, int startIndex, int length)
        {
            if (str.Length <= startIndex + length)
            {
                return str;
            }

            return str.Substring(startIndex, length);
        }

        public static bool ContainsCaseInsensitive(this string source, string toCheck)
        {
            if (toCheck == null)
            {
                throw new ArgumentException("The toCheck string cannot be null");
            }

            return source.IndexOf(toCheck, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }
}
