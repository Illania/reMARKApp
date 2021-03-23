using System;
using Java.Util;

namespace Mark5.Mobile.Droid.Utilities.Extensions
{
    public static class DateTimeExtensions
    {

        public static Calendar ConvertToCalendar(DateTime date)
        {
            Calendar calendar = Calendar.Instance;
            calendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, date.Second);
            return calendar;
        }
    }
}
