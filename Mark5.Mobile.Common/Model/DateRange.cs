using System;

namespace Mark5.Mobile.Common.Model
{
    public class DateRange
    {
        public long StartTimestamp { get; set; } = -1;

        public long EndTimestamp { get; set; } = -1;

        public bool Enabled { get; set; }
    }
}