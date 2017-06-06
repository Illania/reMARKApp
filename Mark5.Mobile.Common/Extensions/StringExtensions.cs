using System;

namespace Mark5.Mobile.Common.Extensions
{
    public static class StringExtensions
    {
        public static string SafeSubstring(this string str, int startIndex)
        {
            if (str.Length <= startIndex)
                return str;

            return str.Substring(startIndex);
        }

        public static string SafeSubstring(this string str, int startIndex, int length)
        {
            if (str.Length <= startIndex + length)
                return str;

            return str.Substring(startIndex, length);
        }

        public static string SafeSubstringBefore(this string str, string v, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            var index = str.IndexOf(v, comparisonType);
            return index > 0 ? str.SafeSubstring(0, index) : str;
        }

        public static string SafeSubstringBeforeLast(this string str, string v, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            var index = str.LastIndexOf(v, comparisonType);
            return index > 0 ? str.SafeSubstring(0, index) : str;
        }

        public static string SafeSubstringAfter(this string str, string v, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            var index = str.IndexOf(v, comparisonType);
            return index >= 0 ? str.SafeSubstring(index + v.Length) : str;
        }

        public static string SafeSubstringAfterLast(this string str, string v, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            var index = str.LastIndexOf(v, comparisonType);
            return index >= 0 ? str.SafeSubstring(index + v.Length) : str;
        }

        public static bool ContainsCaseInsensitive(this string str, string v)
        {
            return str.IndexOf(v, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static string SafeSubstringAfterLast(this string str, char v)
        {
            var index = str.LastIndexOf(v);
            return index >= 0 ? str.SafeSubstring(index + 1) : str;
        }
    }
}