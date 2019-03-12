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
        public string GroupName = "mark5";
        public int StackNotification = -1000;

        public override async void OnMessageReceived(RemoteMessage message)
        {
            try
            {
                var notification = message.ConvertToNotification();

                CommonConfig.Logger.Info($"Notification received: {notification}");

                if (notification.IsSilent)
                    return;

                if (PlatformConfig.Preferences.SilenceNotifications)
                {
                    CommonConfig.Logger.Info($"Notification are silenced - ignoring...");
                    return;
                }

                if (notification.ObjectType == ObjectType.Document)
                {
                    NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
                    CreateChannelIfNotExists(notificationManager);

                    await Managers.NotificationsManager.SaveNotification(notification);

                    Intent intent = DocumentActivity.CreateIntent(this, folderId: notification.FolderId, documentId: notification.ObjectId, notificationGuid: Serializer.Serialize(notification.Guid));
                    PendingIntent pendingIntent = PendingIntent.GetActivity(this, notification.ObjectId, intent, PendingIntentFlags.OneShot);

                    NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this, "main")
                                                   .SetSmallIcon(Resource.Mipmap.ic_icon)
                                                   .SetColor(ContextCompat.GetColor(this, Resource.Color.darkerblue))
                                                   .SetContentTitle(notification.Title)
                                                   .SetContentText(notification.Message)
                                                   .SetContentIntent(pendingIntent)
                                                   .SetCategory(NotificationCompat.CategoryMessage)
                                                   .SetAutoCancel(true).SetGroup(GroupName)
                                                   .SetPriority((int)NotificationPriority.High)
                                                   .SetStyle(new NotificationCompat.BigTextStyle().BigText(notification.Message));

                    if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.NotificationsRingtone))
                        notificationBuilder.SetSound(Android.Net.Uri.Parse(PlatformConfig.Preferences.NotificationsRingtone));

                    if (PlatformConfig.Preferences.NotificationsVibrate)
                        notificationBuilder.SetVibrate(new[] { 500L, 250L, 500L });

                    notificationManager.Notify(notification.ObjectId, notificationBuilder.Build());

                    CommonConfig.MessengerHub.Publish(new NewNotificationsReceivedMessage(this));
                }

                StackIfNeeded();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not process notification. [message.from={message.From}, message.data.keys={string.Join(",", message.Data.Keys)}]", ex);
            }
        }

        void StackIfNeeded()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                return;

            NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
            CreateChannelIfNotExists(notificationManager);

            Android.Service.Notification.StatusBarNotification[] activeNotifications = notificationManager.GetActiveNotifications().Where(sbn => sbn.Id != StackNotification).ToArray();

            if (activeNotifications.Count() < 2)
                return;

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, "main");
            builder.SetSmallIcon(Resource.Mipmap.ic_icon);
            builder.SetColor(ContextCompat.GetColor(this, Resource.Color.darkerblue));

            NotificationCompat.InboxStyle inbox = new NotificationCompat.InboxStyle();

            foreach (Android.Service.Notification.StatusBarNotification sbn in activeNotifications)
            {
                string stackNotificationLine = sbn.Notification.Extras.GetString(NotificationCompat.ExtraTitle);
                if (!string.IsNullOrWhiteSpace(stackNotificationLine))
                    inbox.AddLine(stackNotificationLine);
            }

            inbox.SetSummaryText(activeNotifications.Length.ToString());
            builder.SetStyle(inbox);

            builder.SetCategory(NotificationCompat.CategoryMessage);
            builder.SetAutoCancel(true);
            builder.SetGroup(GroupName);
            builder.SetGroupSummary(true);
            builder.SetPriority((int)NotificationPriority.High);

            Android.App.Notification notification = builder.Build();
            notification.Defaults = 0;

            notificationManager.Notify(StackNotification, notification);
        }

        public void CreateChannelIfNotExists(NotificationManager notificationManager)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            NotificationChannel channel = notificationManager.GetNotificationChannel("main");
            if (channel != null)
                return;

            channel = new NotificationChannel("main", "General", NotificationImportance.High);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}