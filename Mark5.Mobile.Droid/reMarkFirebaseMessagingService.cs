using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Firebase.Messaging;
using Mark5.Mobile.Droid.Ui.Activities;
using WindowsAzure.Messaging;

namespace Mark5.Mobile.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class reMarkFirebaseMessagingService : FirebaseMessagingService
    {
        public reMarkFirebaseMessagingService()
        {
        }

        const string TAG = "reMarkFirebaseMsgService";
        NotificationHub hub;

        public override void OnMessageReceived(RemoteMessage message)
        {
            Common.CommonConfig.Logger.Debug("From: " + message.From);
            var notification = message.GetNotification();
            if ( notification!= null)
            {
                //These is how most messages will be received
                Common.CommonConfig.Logger.Debug("Notification Message Body: " + notification.Body);
                SendNotification(notification.Body);
            }
            else
            {
                //Only used for debugging payloads sent from the Azure portal
                SendNotification(message.Data?.Values.FirstOrDefault());

            }
        }

        void SendNotification(string messageBody)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID);

            notificationBuilder.SetContentTitle("FCM Message")
                        .SetSmallIcon(Resource.Drawable.ic_launcher_foreground)
                        .SetContentText(messageBody)
                        .SetAutoCancel(true)
                        .SetShowWhen(false)
                        .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManager.FromContext(this);

            notificationManager.Notify(0, notificationBuilder.Build());
        }

        public override void OnNewToken(string token)
        {
            Common.CommonConfig.Logger.Debug("FCM token: " + token);
            SendRegistrationToServer(token);
        }

        void SendRegistrationToServer(string token)
        {
            // Register with Notification Hubs
            hub = new NotificationHub(NotificationsConstants.NotificationHubName, NotificationsConstants.ListenConnectionString, this);

            var tags = new List<string>() { };
            var regID = hub.Register(token, tags.ToArray()).RegistrationId;

            Common.CommonConfig.Logger.Debug($"Successful registration of ID {regID}");
        }
    }
}
