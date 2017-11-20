using System;
using Firebase.Analytics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.IOS.Utilities
{
    public class UsageAnalytics : IUsageAnalytics
    {
        public void LogEvent(AnalyticsEvent analyticsEvent)
        {
            try
            {
                Analytics.LogEvent(new NSString(analyticsEvent.EventName), null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while logging usage analytics event", ex);
            }
        }

        public void SetUserProperty(UserProperty property, string value)
        {
            try
            {
                Analytics.SetUserProperty(new NSString(value), new NSString(property.ToString()));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while setting user property", ex);
            }
        }
    }
}