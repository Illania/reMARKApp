using Android.OS;
using Firebase.Analytics;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Extensions;

namespace reMark.Mobile.Droid.Utilities
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
              
                if (analyticsEvent.EventName.Length > 35)
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

                firebaseAnalytics.LogEvent(analyticsEvent.EventName.SafeSubstring(0, 35), bundle);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while logging analytics event", ex);
            }

        }

        public void SetUserProperty(UserProperty property, string value)
        {
            try
            {
                firebaseAnalytics.SetUserProperty(property.ToString().ToLowerInvariant().SafeSubstring(0, 35), value);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while setting user property", ex);
            }

        }
    }
}
