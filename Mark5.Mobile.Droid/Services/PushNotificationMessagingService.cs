using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Firebase.Messaging;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model.HubMessages;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Utilities.Services
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
                    await Managers.NotificationsManager.SaveNotification(n);

                    var i = new Intent(this, typeof(DocumentActivity));
                    i.PutExtra(DocumentActivity.FolderIdIntentKey, n.FolderId);
                    i.PutExtra(DocumentActivity.DocumentIdIntentKey, n.ObjectId);
                    i.PutExtra(DocumentActivity.NotificationGuidIntentKey, Serializer.Serialize(n.Guid));
                    var pi = PendingIntent.GetActivity(this, 0, i, PendingIntentFlags.OneShot);

                    var nb = new NotificationCompat.Builder(this).SetSmallIcon(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ? Resource.Mipmap.ic_icon_lollipop : Resource.Mipmap.ic_icon).SetColor(ContextCompat.GetColor(this, Resource.Color.darkerblue)).SetContentTitle(n.Title).SetContentText(n.Message).SetContentIntent(pi).SetCategory(Android.Support.V4.App.NotificationCompat.CategoryMessage).SetAutoCancel(true).SetGroup(GroupName).SetPriority((int)NotificationPriority.High).SetStyle(new Android.Support.V4.App.NotificationCompat.BigTextStyle().BigText(n.Message));

                    if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.NotificationsRingtone))
                        nb.SetSound(Android.Net.Uri.Parse(PlatformConfig.Preferences.NotificationsRingtone));

                    if (PlatformConfig.Preferences.NotificationsVibrate)
                        nb.SetVibrate(new[]
                        {
                            500L,
                            250L,
                            500L
                        });

                    var nm = Android.Support.V4.App.NotificationManagerCompat.From(this);
                    nm.Notify((int)(Java.Lang.JavaSystem.CurrentTimeMillis() / 1000), nb.Build());

                    PlatformConfig.MessengerHub.Publish(new NewNotificationsReceived(this));
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

            var activeNotifications = nm.GetActiveNotifications().Where(sbn => sbn.Id != StackNotification).ToArray();

            if (activeNotifications.Count() < 2)
                return;

            var builder = new NotificationCompat.Builder(this);
            builder.SetSmallIcon(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ? Resource.Mipmap.ic_icon_lollipop : Resource.Mipmap.ic_icon);
            builder.SetColor(ContextCompat.GetColor(this, Resource.Color.darkerblue));

            var inbox = new Android.Support.V4.App.NotificationCompat.InboxStyle();

            foreach (var sbn in activeNotifications)
            {
                var stackNotificationLine = sbn.Notification.Extras.GetString(Android.Support.V4.App.NotificationCompat.ExtraTitle);
                if (!string.IsNullOrWhiteSpace(stackNotificationLine))
                    inbox.AddLine(stackNotificationLine);
            }

            inbox.SetSummaryText(activeNotifications.Length.ToString());
            builder.SetStyle(inbox);

            builder.SetCategory(Android.Support.V4.App.NotificationCompat.CategoryMessage);
            builder.SetAutoCancel(true);
            builder.SetGroup(GroupName);
            builder.SetGroupSummary(true);
            builder.SetPriority((int)NotificationPriority.High);

            var n = builder.Build();
            n.Defaults = 0;

            nm.Notify(GroupName, StackNotification, n);
        }
    }
}