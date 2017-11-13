using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Analytics;

namespace Mark5.Mobile.IOS.Utilities
{
    public class AnalyticsImplementation : IAnalyticsImplementation
    {
        public void LogEvent(AnalyticsEvent analyticsEvent)
        {
            try
            {
                NSDictionary<NSString, NSObject> parameters = null;
                if (analyticsEvent.Parameters?.Any() == true)
                {
                    var parametersDic = new Dictionary<NSString, NSObject>();
                    foreach (var parameter in analyticsEvent.Parameters)
                    {
                        if (parameter is StringAnalyticsParameter stringParameter)
                        {
                            parametersDic.Add(new NSString(stringParameter.Name), new NSString(stringParameter.Value));
                        }
                        else if (parameter is NumberAnalyticsParameter numberParameter)
                        {
                            parametersDic.Add(new NSString(numberParameter.Name), new NSNumber(numberParameter.Value));
                        }
                    }
                    parameters = new NSDictionary<NSString, NSObject>(parametersDic.Keys.ToArray(), parametersDic.Values.ToArray());
                }
                Analytics.LogEvent(new NSString(analyticsEvent.Name), parameters);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while logging analytics event", ex);
            }
        }
    }
}
