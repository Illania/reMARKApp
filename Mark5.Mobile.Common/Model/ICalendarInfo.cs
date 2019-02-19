using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class ICalendarInfo
    {
        public List<IEventInfo> Events { get; set; } = new List<IEventInfo>();
        public ICalendarInfoMethodType Method { get; set; } = ICalendarInfoMethodType.Reply;
        public IReplyInfo Reply { get; set; } = null;
    }
}