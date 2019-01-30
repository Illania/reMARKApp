using System;
using Android.App;
using Android.Content;
using Firebase.Messaging;
using Mark5.Mobile.Common;

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
                var notification = message.ConvertToNotification();
                await PushNotificationsManager.ProcessMessageReceived(this, notification);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not process notification. [message.from={message.From}, message.data.keys={string.Join(",", message.Data.Keys)}]", ex);
            }
        }
    }
}