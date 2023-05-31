using System;
using Android.App;
using Android.Content;
using Firebase.Messaging;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Utilities;
using Microsoft.Extensions.Logging;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Service
{
    [Service(Exported = true)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FCMService : FirebaseMessagingService
    {
        public override async void OnMessageReceived(RemoteMessage message)
        {
            try
            {
                var notification = message.ConvertToNotification();
                await PushNotificationsUtilities.ProcessMessageReceived(this, notification);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not process notification. [message.from={message.From}, message.data.keys={string.Join(",", message.Data.Keys)}]", ex);
                CommonConfig.Sentry.LogError($"Could not process notification. [message.from={message.From}, message.data.keys={string.Join(",", message.Data.Keys)}]", ex);

            }
        }

        
       public override void OnNewToken(string newToken)
       {
            try
            {

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Received Firebase token: {newToken}");

                CommonConfig.Sentry?.LogInformation($"Received Firebase token: {newToken}");

                var oldToken = PlatformConfig.Preferences.PushNotificationToken;
                PlatformConfig.Preferences.PushNotificationToken = newToken;

                if (Managers.ActiveConnectionInfo != null)
                {
                    CommonConfig.Logger.Info($"Sending Firebase token to service...");

                    if (!string.IsNullOrWhiteSpace(oldToken) && oldToken != newToken)
                    {
                        CommonConfig.Logger.Info($"New Firebase token is different, so try to unsubscribe old one...");
                        CommonConfig.Sentry?.LogInformation($"New Firebase token is different, so try to unsubscribe old one...");

                        Managers.NotificationsManager.UnSubscribe(DeviceType.Android, oldToken).FireAndForget();
                    }

                    Managers.NotificationsManager.Subscribe(DeviceType.Android, newToken).FireAndForget();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while refreshing Firebase token", ex);
                CommonConfig.Sentry?.LogError("Error while refreshing Firebase token", ex);
            }
        }
    }
}