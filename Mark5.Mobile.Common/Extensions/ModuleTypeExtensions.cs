using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Extensions
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
                case ModuleType.Calendar:
                    return new[]
                    {
                        ObjectType.CalendarTask,
                        ObjectType.CalendarAppointment
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
                case ObjectType.CalendarTask:
                    return ModuleType.Calendar;
                case ObjectType.CalendarAppointment:
                    return ModuleType.Calendar;
                default:
                    return ModuleType.None;
            }
        }
    }
}