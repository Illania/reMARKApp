//
// Project: Mark5.Mobile.Common
// File: StringExtensions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

#pragma warning disable CS1701
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
    }
}
