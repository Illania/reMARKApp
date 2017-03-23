//
// Project: Mark5.Mobile.Droid
// File: ModuleTypeExtensions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Extensions
{

    public static class ModuleTypeExtensions
    {

        public static ObjectType[] ObjectTypes(this ModuleType module)
        {
            switch (module)
            {
                case ModuleType.Documents:
                    return new[] { ObjectType.Document };
                case ModuleType.Contacts:
                    return new[] { ObjectType.Contact };
                case ModuleType.Shortcodes:
                    return new[] { ObjectType.Shortcode };
                case ModuleType.Calendar:
                    return new[] { ObjectType.CalendarTask, ObjectType.CalendarAppointment };
                default:
                    return new ObjectType[0];
            }
        }
    }
}
