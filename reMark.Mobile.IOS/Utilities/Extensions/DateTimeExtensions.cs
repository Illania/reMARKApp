using System;
using Foundation;

namespace reMark.Mobile.IOS.Utilities.Extensions
{
    public static class DateTimeExtensions
    {
        public static NSDate ToNSDate(this DateTime date, DateTimeKind dateTimeKind)
        {
            if (date.Kind == DateTimeKind.Unspecified)
                date = DateTime.SpecifyKind(date, dateTimeKind);
            return (NSDate)date;
        }
    }
}
