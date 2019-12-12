using System;
using Android.App;
using Android.Util;
using Java.Util;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class Conversion
    {
        public static int ConvertDpToPixels(float dp)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, Application.Context.Resources.DisplayMetrics);
        }

        public static Calendar ConvertToCalendar(this DateTime date)
        {
            Calendar calendar = Calendar.Instance;
            calendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, date.Second);
            return calendar;
        }

        public static DateTime ConvertToDateTime(this Calendar date)
        {
            var year = date.Get(CalendarField.Year);
            var month = date.Get(CalendarField.Month) + 1;
            var day = date.Get(CalendarField.Date);
            var hour = date.Get(CalendarField.HourOfDay);
            var minute = date.Get(CalendarField.Minute);
            var second = date.Get(CalendarField.Second);
            var dateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);

            return dateTime;
        }

    }
}