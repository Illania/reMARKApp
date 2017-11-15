namespace Mark5.Mobile.Common.Analytics
{
    public static class AnalyticsManager //TODO need a different name
    {
        static IAnalytics implementation;

        public static void Initialize(IAnalytics im)
        {
            implementation = im;
        }

        public static void LogEvent(AnalyticsEvent analyticsEvent)
        {
            implementation.LogEvent(analyticsEvent);
        }
    }


}
