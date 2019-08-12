using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities.Extensions
{
    public static class ModelEnumExtensions
    {
        public static string GetDayName(this WeekDays val)
        {
            switch (val)
            {
                case WeekDays.Monday: return "Monday";
                case WeekDays.Tuesday: return "Tuesday";
                case WeekDays.Wednesday: return "Wednesday";
                case WeekDays.Thursday: return "Thursday";
                case WeekDays.Friday: return "Friday";
                case WeekDays.Saturday: return "Saturday";
                case WeekDays.Sunday: return "Sunday";
                case WeekDays.WeekendDays: return "Weekend day";
                case WeekDays.EveryDay: return "Day";
                case WeekDays.WorkDays: return "Weekday";
                default: return string.Empty;
            }
        }
    }
}
