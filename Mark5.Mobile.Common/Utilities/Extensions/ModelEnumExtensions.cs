using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities.Extensions
{
    public static class ModelEnumExtensions
    {
        public static string ToFriendlyString(this WeekDays val)
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

        public static string ToFriendlyString(this RecurrenceType val)
        {
            switch (val)
            {
                case RecurrenceType.Daily: return "Daily";
                case RecurrenceType.Weekly: return "Weekly";
                case RecurrenceType.Monthly: return "Monthly";
                case RecurrenceType.Yearly: return "Yearly";
                default: return string.Empty;
            }
        }

        public static string ToFriendlyString(this WeekOfMonth val)
        {
            switch (val)
            {
                case WeekOfMonth.First: return "First";
                case WeekOfMonth.Second: return "Second";
                case WeekOfMonth.Third: return "Third";
                case WeekOfMonth.Fourth: return "Fourth";
                case WeekOfMonth.Last: return "Last";
                default: return string.Empty;
            }
        }
    }
}