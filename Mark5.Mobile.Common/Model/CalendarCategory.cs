using System;

namespace Mark5.Mobile.Common.Model
{
    public class CalendarCategory
    {
        public int Id { get; set; } = -1;

        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ColorHex { get; set; }
        public CalendarCategoryType Type { get; set; }
        public CalendarCategorySubType SubType { get; set; }
    }
}