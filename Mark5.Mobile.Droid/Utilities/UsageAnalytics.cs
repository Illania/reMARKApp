using System;
using System.Linq;
using Android.OS;
using Firebase.Analytics;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Extensions;

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
                        if (parameter.Value is string stringParameter)
                        {
                            bundle.PutString(parameter.Key, stringParameter);
                        }
                        if (parameter.Value is long numberParameter)
                        {
                            bundle.PutLong(parameter.Key, numberParameter);
                        }
                    }
                }

                firebaseAnalytics.LogEvent(analyticsEvent.EventName.SafeSubstring(0, 40), bundle);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while logging analytics event", ex);
            }

        }

        public void SetScreen(string screenClass)
        {
            //Not implemented for Android 
        }

        public void SetUserProperty(UserProperty property, string value)
        {
            try
            {
                firebaseAnalytics.SetUserProperty(property.ToString().ToLowerInvariant(), value);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while setting user property", ex);
            }

        }
    }
}
