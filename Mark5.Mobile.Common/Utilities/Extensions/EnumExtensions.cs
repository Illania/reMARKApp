using System;

namespace Mark5.Mobile.Common.Utilities.Extensions
{
    public static class EnumExtensions
    {
        public static bool HasAnyFlag(this Enum e, params Enum[] flags)
        {
            foreach (var f in flags)
                if (e.HasFlag(f))
                    return true;

            return false;
        }

        public static bool HasAllFlags(this Enum e, params Enum[] flags)
        {
            foreach (var f in flags)
                if (!e.HasFlag(f))
                    return false;

            return true;
        }
    }
}
