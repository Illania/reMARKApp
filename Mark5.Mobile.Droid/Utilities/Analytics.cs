using System.Linq;
using Android.OS;
using Firebase.Analytics;
using Mark5.Mobile.Common.Model.AnalyticsEvents;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class Analytics
    {
        static FirebaseAnalytics firebaseAnalytics;

        public static void Initialize(FirebaseAnalytics fa)
        {
            firebaseAnalytics = fa;
        }

        public static void LogEvent(AnalyticsEvent analyticsEvent)
        {
            //TODO need to put a try catch
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
    }
}
