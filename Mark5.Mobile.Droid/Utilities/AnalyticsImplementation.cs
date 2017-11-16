using System;
using System.Linq;
using Android.OS;
using Firebase.Analytics;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Analytics;

namespace Mark5.Mobile.Droid.Utilities
{
    public class AnalyticsImplementation : IAnalytics
    {
        readonly FirebaseAnalytics firebaseAnalytics;

        public AnalyticsImplementation(FirebaseAnalytics firebaseAnalytics)
        {
            this.firebaseAnalytics = firebaseAnalytics;
        }

        public void LogEvent(AnalyticsEvent analyticsEvent)
        {
            try
            {
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
    }
}
