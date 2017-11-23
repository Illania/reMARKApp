using System;
using System.Linq;
using Android.OS;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Utilities
{
    public class UsageAnalytics : IUsageAnalytics
    {
        readonly FirebaseAnalytics firebaseAnalytics;

        public UsageAnalytics(FirebaseAnalytics firebaseAnalytics)
        {
            this.firebaseAnalytics = firebaseAnalytics;
        }

        public void LogEvent(AnalyticsEvent analyticsEvent)
        {
            try
            {
                if (analyticsEvent.EventName.Length > 40)
                    CommonConfig.Logger.Error($"Event name is too long! [{analyticsEvent.EventName}]");

                Bundle bundle = null;
                if (analyticsEvent.Parameters?.Any() == true)
                {
                    bundle = new Bundle();
                    foreach (var parameter in analyticsEvent.Parameters)
                    {
                        if (parameter is StringAnalyticsParameter stringParameter)
                        {
                            bundle.PutString(stringParameter.Name, stringParameter.Value);
                        }
                        else if (parameter is NumberAnalyticsParameter numberParameter)
                        {
                            bundle.PutLong(numberParameter.Name, numberParameter.Value);
                        }
                    }
                }
                firebaseAnalytics.LogEvent(analyticsEvent.Name, bundle);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while logging analytics event", ex);
            }

        }

        public void SetScreen(string screenClass)
        {
            throw new NotImplementedException();
        }

        public void SetUserProperty(UserProperty property, string value)
        {
            firebaseAnalytics.SetUserProperty(property.ToString(), value);
        }
    }
}
