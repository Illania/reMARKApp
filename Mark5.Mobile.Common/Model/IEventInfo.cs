using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class IEventInfo
    {
        public List<IAttendeeInfo> Attendees { get; set; } = new List<IAttendeeInfo>();
        public string Description { get; set; } = string.Empty;
        public DateTime End { get; set; } = DateTime.MinValue;
        public string Id { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Start { get; set; } = DateTime.MinValue;
        public string Summary { get; set; } = string.Empty;
    }
}
