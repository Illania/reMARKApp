namespace Mark5.Mobile.Common.Analytics
{
    public static class AnalyticsManager
    {
        static IAnalyticsImplementation implementation;

        public static void Initialize(IAnalyticsImplementation im)
        {
            implementation = im;
        }

        public static void LogEvent(AnalyticsEvent analyticsEvent)
        {
            implementation.LogEvent(analyticsEvent);
        }
    }

    public interface IAnalyticsImplementation
    {
        void LogEvent(AnalyticsEvent analyticsEvent);
    }
}
