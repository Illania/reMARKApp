using System;
using System.Collections.Generic;
using System.Globalization;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;

namespace Mark5.Mobile.Common.Model
{
    public class RecurrenceInfo
    {
        public bool AllDay { get; set; }  //NOT USED

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

        public DateTime StartDate
        {
            get => StartTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
            set => StartTimestamp = value.ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();
        }

        public DateTime EndDate
        {
            get => EndTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
            set => EndTimestamp = value.ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();
        }

        public string ToFriendlyString()
        {
            string pattern = "Reoccurs ";
            string range = string.Empty;

            string GetMonthString(int val)
            {
                return CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(val);
            }

            switch (Type)
            {
                case RecurrenceType.Daily:
                    pattern += "daily, every ";

                    if (WeekDays == WeekDays.EveryDay)
                        pattern += $"{Periodicity} day(s)";
                    else //ri.WeekDays == WeekDays.WorkDays
                        pattern += $"weekday";
                    break;
                case RecurrenceType.Weekly:
                    pattern += $"weekly, every {Periodicity} week(s) on ";

                    var days = new[] { WeekDays.Monday, WeekDays.Tuesday, WeekDays.Wednesday,
                    WeekDays.Thursday, WeekDays.Friday, WeekDays.Saturday, WeekDays.Sunday};

                    var stringDays = new List<string>();

                    foreach (var day in days)
                    {
                        if (WeekDays.HasFlag(day))
                            stringDays.Add(day.ToFriendlyString());
                    }

                    pattern += string.Join(", ", stringDays);
                    break;
                case RecurrenceType.Monthly:
                    pattern += $"monthly, ";
                    if (WeekOfMonth == WeekOfMonth.None)
                    {
                        string monthPatter = Periodicity == 1 ? "month" : $"{Periodicity} months";
                        pattern += $"on day {DayNumber} of every {monthPatter}";
                    }
                    else
                        pattern += $"the {WeekOfMonth.ToFriendlyString()} {WeekDays.ToFriendlyString()} of every {Periodicity} month(s)";
                    break;
                case RecurrenceType.Yearly:
                    pattern += $"Yearly, ";

                    if (WeekOfMonth == WeekOfMonth.None)
                        pattern += $"every {GetMonthString(Month)}, {DayNumber}";
                    else
                        pattern += $"the {WeekOfMonth.ToFriendlyString()} {WeekDays.ToFriendlyString()} of {GetMonthString(Month)}";
                    break;
            }

            switch (Range)
            {
                case RecurrenceRange.NoEndDate:
                    range = string.Empty;
                    break;
                case RecurrenceRange.OccurrenceCount:
                    range = $", ends after {OccurrenceCount} occurrences";
                    break;
                case RecurrenceRange.EndByDate:
                    range = $", ends by {EndDate.ToString("d", CultureInfo.CurrentCulture)}";
                    break;
            }

            return pattern + range;
        }

    }
}
