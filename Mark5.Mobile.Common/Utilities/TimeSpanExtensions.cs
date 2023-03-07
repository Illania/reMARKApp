using System;
namespace Mark5.Mobile.Common.Utilities
{
    public static class TimeSpanExtensions
    {
        public static string ToReadableString(this TimeSpan span)
        {
            var formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? $"{span.Days:0} day{(span.Days == 1 ? string.Empty : "s")}, " : string.Empty,
                span.Duration().Hours > 0 ? $"{span.Hours:0} hour{(span.Hours == 1 ? string.Empty : "s")}, " : string.Empty,
                span.Duration().Minutes > 0 ? $"{span.Minutes:0} minute{(span.Minutes == 1 ? string.Empty : "s")}, " : string.Empty,
                span.Duration().Seconds > 0 ? $"{span.Seconds:0} second{(span.Seconds == 1 ? string.Empty : "s")}" : string.Empty);

            return string.IsNullOrEmpty(formatted) ? "0 seconds" : formatted.TrimEnd(',');
        }
    }
}
