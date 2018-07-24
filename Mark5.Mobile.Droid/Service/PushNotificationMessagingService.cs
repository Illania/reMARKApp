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
                    var nm = (NotificationManager)GetSystemService(NotificationService);
                    CreateChannelIfNotExists(nm);

                    await Managers.NotificationsManager.SaveNotification(n);

                    var i = DocumentActivity.CreateIntent(this, folderId: n.FolderId, documentId: n.ObjectId, notificationGuid: Serializer.Serialize(n.Guid));
                    var pi = PendingIntent.GetActivity(this, 0, i, PendingIntentFlags.OneShot);

                    var nb = new NotificationCompat.Builder(this, "main")
                                                   .SetSmallIcon(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ? Resource.Mipmap.ic_icon_lollipop : Resource.Mipmap.ic_icon)
                                                   .SetColor(ContextCompat.GetColor(this, Resource.Color.darkerblue))
                                                   .SetContentTitle(n.Title)
                                                   .SetContentText(n.Message)
                                                   .SetContentIntent(pi)
                                                   .SetCategory(NotificationCompat.CategoryMessage)
                                                   .SetAutoCancel(true).SetGroup(GroupName)
                                                   .SetPriority((int)NotificationPriority.High)
                                                   .SetStyle(new NotificationCompat.BigTextStyle().BigText(n.Message));

                    if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.NotificationsRingtone))
                        nb.SetSound(Android.Net.Uri.Parse(PlatformConfig.Preferences.NotificationsRingtone));

                    if (PlatformConfig.Preferences.NotificationsVibrate)
                        nb.SetVibrate(new[] { 500L, 250L, 500L });

                    nm.Notify((int)(Java.Lang.JavaSystem.CurrentTimeMillis() / 1000), nb.Build());

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

            var nm = (NotificationManager)GetSystemService(NotificationService);
            CreateChannelIfNotExists(nm);

            var activeNotifications = nm.GetActiveNotifications().Where(sbn => sbn.Id != StackNotification).ToArray();

            if (activeNotifications.Count() < 2)
                return;

            var builder = new NotificationCompat.Builder(this, "main");
            builder.SetSmallIcon(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ? Resource.Mipmap.ic_icon_lollipop : Resource.Mipmap.ic_icon);
            builder.SetColor(ContextCompat.GetColor(this, Resource.Color.darkerblue));

            var inbox = new NotificationCompat.InboxStyle();

            foreach (var sbn in activeNotifications)
            {
                var stackNotificationLine = sbn.Notification.Extras.GetString(NotificationCompat.ExtraTitle);
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

            var n = builder.Build();
            n.Defaults = 0;

            nm.Notify(GroupName, StackNotification, n);
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