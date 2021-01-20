using System;
using Android.App;
using Android.Content;
using Firebase.Messaging;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Service
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseMessagingService : Firebase.Messaging.FirebaseMessagingService
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
            }
        }
    }
}