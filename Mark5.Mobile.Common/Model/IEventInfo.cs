using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class IEventInfo
    {
        public List<IAttendeeInfo> Attendees { get; set; } = new List<IAttendeeInfo>();
        public string Description { get; set; }
        public DateTime End { get; set; }
        public string Id { get; set; }
        public string Location { get; set; }
        public DateTime Start { get; set; }
        public string Summary { get; set; }
    }
}
