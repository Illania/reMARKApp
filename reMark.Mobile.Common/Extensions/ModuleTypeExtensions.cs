using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.Extensions
{
    public static class ModuleTypeExtensions
    {
        public static ObjectType[] ObjectTypes(this ModuleType module)
        {
            switch (module)
            {
                case ModuleType.Documents:
                    return new[]
                    {
                        ObjectType.Document
                    };
                case ModuleType.Contacts:
                    return new[]
                    {
                        ObjectType.Contact
                    };
                case ModuleType.Shortcodes:
                    return new[]
                    {
                        ObjectType.Shortcode
                    };
                default:
                    return new ObjectType[0];
            }
        }

        public static ModuleType Module(this ObjectType type)
        {
            switch (type)
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