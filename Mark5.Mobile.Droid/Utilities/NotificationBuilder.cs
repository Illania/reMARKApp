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
    public static class NotificationBuilder
    {
        const string ReceivedEmailGroupKey = "b651c305-2250-4572-9528-d79275dfc86f";
        const string SendingEmailFailedGroupKey = "9bfb0b34-516b-433b-a70e-121094d8f9c1";

        const string EmailChannelId = "c1b5a3f4-9097-422d-bd23-8ee751ee47fd";
        const string CalendarChannelId = "bf1f3f92-2fa3-44d9-b095-2c1e1a9d6809";

        const string ReceivedEmailCategory = "Received emails";

        const int StackNotification = -1000;

        public static void EmailReceived(Context context, Common.Model.Notification notification)
        {
            var nm = (NotificationManager)context.GetSystemService(Context.NotificationService);
            CreateChannelIfNotExists(context,EmailChannelId,ReceivedEmailCategory, NotificationImportance.High);

            var i = DocumentActivity.CreateIntent(context, folderId: notification.FolderId, documentId: notification.ObjectId, notificationGuid: Serializer.Serialize(notification.Guid));
            var pi = PendingIntent.GetActivity(context, 0, i, PendingIntentFlags.OneShot);

            var nb = new NotificationCompat.Builder(context, EmailChannelId)
                                           .SetSmallIcon(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ? Resource.Mipmap.ic_icon_lollipop : Resource.Mipmap.ic_icon)
                                           .SetColor(ContextCompat.GetColor(context, Resource.Color.darkerblue))
                                           .SetContentTitle(notification.Title)
                                           .SetContentText(notification.Message)
                                           .SetContentIntent(pi)
                                           .SetCategory(NotificationCompat.CategoryMessage)
                                           .SetAutoCancel(true)
                                           .SetGroup(ReceivedEmailGroupKey)
                                           .SetNumber(1)
                                           .SetPriority((int)NotificationPriority.High)
                                           .SetStyle(new NotificationCompat.BigTextStyle().BigText(notification.Message));

            if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.NotificationsRingtone))
                nb.SetSound(Android.Net.Uri.Parse(PlatformConfig.Preferences.NotificationsRingtone));

            if (PlatformConfig.Preferences.NotificationsVibrate)
                nb.SetVibrate(new[] { 500L, 250L, 500L });
            
            nm.Notify((int)(Java.Lang.JavaSystem.CurrentTimeMillis() / 1000), nb.Build());

            StackIfNeeded(context, EmailChannelId, ReceivedEmailGroupKey);
        }

        static void StackIfNeeded(Context context, string channelId, string categoryName)
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
            builder.SetGroup(categoryName);
            builder.SetGroupSummary(true);
            builder.SetPriority((int)NotificationPriority.High);

            var n = builder.Build();
            n.Defaults = 0;

            nm.Notify(categoryName, StackNotification, n);
        }

        public static void CreateChannelIfNotExists(Context context, string channelId, string categoryName, NotificationImportance importance)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var nm = (NotificationManager)context.GetSystemService(Context.NotificationService);

            var channel = nm.GetNotificationChannel(channelId);
            if (channel != null)
                return;

            channel = new NotificationChannel(channelId, categoryName, importance);
            nm.CreateNotificationChannel(channel);
        }
    }
}