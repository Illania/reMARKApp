using System.Collections.Generic;

namespace Mark5.Mobile.Common.Utilities.Extensions
{
    public static class StringExtensions
    {

        static readonly Dictionary<char, char> replaceCharacters = new Dictionary<char, char>
        {
            {'„','"' },
            {'“','"' },
            {'”','"' }
        };

        public static string SanitizeForSearch(this string str)
        {
            if (str == null)
                return null;

            var result = str;

            foreach (var kv in replaceCharacters)
                result = result.Replace(kv.Key, kv.Value);

            return result;
        }
    }
}
