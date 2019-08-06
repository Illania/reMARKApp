using System;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.Model
{
    public class RecurrenceInfo
    {
        public bool AllDay { get; set; }

        public int DayNumber { get; set; } = -1;

        public TimeSpan Duration { get; set; } //When using EndByDate End = Start + Duration

        public long StartTimestamp { get; set; }

        public long EndTimestamp { get; set; }

        public DayOfWeek FirstDayOfWeek { get; set; }

        public int Month { get; set; } = -1;

        public int OccurrenceCount { get; set; } = -1;

        public int Periodicity { get; set; } = -1;

        public RecurrenceRange Range { get; set; }

        public RecurrenceType Type { get; set; }

        public WeekDays WeekDays { get; set; }

        public WeekOfMonth WeekOfMonth { get; set; }

        public DateTime StartDate => StartTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
        public DateTime EndDate => EndTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
    }
}
