using System;
namespace reMark.Mobile.Common.Model
{
    public static class EnumExtensions
    {
        public static ModuleType GetModuleTypeForObjectType(this ObjectType objectType)
        {
            switch (objectType)
            {
                case ObjectType.Document:
                    return ModuleType.Documents;
                case ObjectType.Contact:
                    return ModuleType.Contacts;
                case ObjectType.Shortcode:
                    return ModuleType.Shortcodes;
                case ObjectType.None:
                    return ModuleType.None;
                default:
                    throw new InvalidOperationException("The provided enum type doesn't exist!");
            }
        }
    }
}