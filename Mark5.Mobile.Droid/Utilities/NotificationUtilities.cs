using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class NotificationUtilities
    {
        const string ReceivedEmailGroupKey = "b651c305-2250-4572-9528-d79275dfc86f";
        const string ReceivedEmailChannelId = "c1b5a3f4-9097-422d-bd23-8ee751ee47fd";
        const string ReceivedEmailChannelName = "Received emails";

        const int StackNotification = -1000;

        public static void EmailReceived(Context context, Common.Model.Notification notification)
        {
            var nm = (NotificationManager)context.GetSystemService(Context.NotificationService);
            CreateChannelIfNotExists(context,ReceivedEmailChannelId,ReceivedEmailChannelName, NotificationImportance.High);

            var i = DocumentActivity.CreateIntent(context, folderId: notification.FolderId, documentId: notification.ObjectId, notificationGuid: Serializer.Serialize(notification.Guid));
            var pi = PendingIntent.GetActivity(context, 0, i, PendingIntentFlags.OneShot);

            var nb = new NotificationCompat.Builder(context, ReceivedEmailChannelId)
                                           .SetSmallIcon(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ? Resource.Mipmap.ic_icon_lollipop : Resource.Mipmap.ic_icon)
                                           .SetColor(ContextCompat.GetColor(context, Resource.Color.darkerblue))
                                           .SetContentTitle(notification.Title)
                                           .SetContentText(notification.Message)
                                           .SetContentIntent(pi)
                                           .SetCategory(NotificationCompat.CategoryMessage)
                                           .SetAutoCancel(true)
                                           .SetGroup(ReceivedEmailGroupKey)
                                           .SetPriority((int)NotificationPriority.High)
                                           .SetStyle(new NotificationCompat.BigTextStyle().BigText(notification.Message));

            if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.NotificationsRingtone))
                nb.SetSound(Android.Net.Uri.Parse(PlatformConfig.Preferences.NotificationsRingtone));

            if (PlatformConfig.Preferences.NotificationsVibrate)
                nb.SetVibrate(new[] { 500L, 250L, 500L });
            
            nm.Notify((int)(Java.Lang.JavaSystem.CurrentTimeMillis() / 1000), nb.Build());

            StackIfNeeded(context, ReceivedEmailChannelId, ReceivedEmailGroupKey);
        }

        static void StackIfNeeded(Context context, string channelId, string channelName)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                return;

            var nm = (NotificationManager)context.GetSystemService(Context.NotificationService);

            var activeNotifications = nm.GetActiveNotifications().Where(sbn => sbn.Id != StackNotification).ToArray();

            if (activeNotifications.Count() < 2)
                return;

            var builder = new NotificationCompat.Builder(context, channelId);
            builder.SetSmallIcon(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ? Resource.Mipmap.ic_icon_lollipop : Resource.Mipmap.ic_icon);
            builder.SetColor(ContextCompat.GetColor(context, Resource.Color.darkerblue));

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
            builder.SetGroup(channelName);
            builder.SetGroupSummary(true);
            builder.SetPriority((int)NotificationPriority.High);

            var n = builder.Build();
            n.Defaults = 0;

            nm.Notify(channelName, StackNotification, n);
        }

        static void CreateChannelIfNotExists(Context context, string channelId, string channelName, NotificationImportance importance)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var nm = (NotificationManager)context.GetSystemService(Context.NotificationService);

            var channel = nm.GetNotificationChannel(channelId);
            if (channel != null)
                return;

            channel = new NotificationChannel(channelId, channelName, importance);
            nm.CreateNotificationChannel(channel);
        }
    }
}