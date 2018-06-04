using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Firebase.Messaging;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Utilities.Service
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PushNotificationMessagingService : FirebaseMessagingService
    {
        public override async void OnMessageReceived(RemoteMessage message)
        {
            try
            {
                var n = message.ConvertToNotification();

                CommonConfig.Logger.Info($"Notification received: {n}");

                if (n.IsSilent)
                    return;
                
                if (PlatformConfig.Preferences.SilenceNotifications)
                {
                    CommonConfig.Logger.Info($"Notification are silenced - ignoring...");
                    return;
                }

                if (n.ObjectType == ObjectType.Document)
                {
                    NotificationBuilder.EmailReceived(this, n);
                    CommonConfig.MessengerHub.Publish(new NewNotificationsReceivedMessage(this));
                }

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not process notification. [message.from={message.From}, message.data.keys={string.Join(",", message.Data.Keys)}]", ex);
            }
        }

        public void CreateChannelIfNotExists(NotificationManager nm)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var channel = nm.GetNotificationChannel("main");
            if (channel != null)
                return;

            channel = new NotificationChannel("main", "General", NotificationImportance.High);
            nm.CreateNotificationChannel(channel);
        }
    }
}