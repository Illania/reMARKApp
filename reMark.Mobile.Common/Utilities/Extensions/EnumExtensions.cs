using System;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.Utilities.Extensions
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

        public static ModuleType ToModuleType(this ObjectType ot)
        {
            switch (ot)
            {
                case ObjectType.Document:
                    return ModuleType.Documents;
                case ObjectType.Contact:
                    return ModuleType.Contacts;
                case ObjectType.Shortcode:
                    return ModuleType.Shortcodes;
                default:
                    return ModuleType.None;
            }
        }

    }
}
