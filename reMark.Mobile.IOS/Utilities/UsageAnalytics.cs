using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Extensions;

namespace reMark.Mobile.IOS.Utilities
{
    public class UsageAnalytics : IUsageAnalytics
    {
        public void LogEvent(AnalyticsEvent analyticsEvent)
        {
            try
            {
                if (analyticsEvent.EventName.Length > 35)
                    CommonConfig.Logger.Error($"Event name is too long! [{analyticsEvent.EventName}]");

                NSDictionary<NSString, NSObject> parameters = null;
                if (analyticsEvent.Parameters?.Any() == true)
                {
                    var parametersDic = new Dictionary<NSString, NSObject>();
                    foreach (var parameter in analyticsEvent.Parameters)
                    {
                        if (parameter.Value is string stringParameter)
                        {
                            parametersDic.Add(new NSString(parameter.Key), new NSString(stringParameter));
                        }
                        if (parameter.Value is long numberParameter)
                        {
                            parametersDic.Add(new NSString(parameter.Key), new NSNumber(numberParameter));
                        }
                    }
                    parameters = new NSDictionary<NSString, NSObject>(parametersDic.Keys.ToArray(), parametersDic.Values.ToArray());
                }

                Analytics.LogEvent(new NSString(analyticsEvent.EventName.SafeSubstring(0, 35)), parameters);
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
                Analytics.SetUserProperty(new NSString(value), new NSString(property.ToString().ToLowerInvariant().SafeSubstring(0, 35)));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while setting user property", ex);
            }
        }

    }
}