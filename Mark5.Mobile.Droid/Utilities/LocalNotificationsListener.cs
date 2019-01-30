using System;
using Mark5.Mobile.Common.Manager;
using Android.Support.V4.App;
using Android.OS;
using Android.Support.V4.Content;
using Android.App;
using Android.Content;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class LocalNotificationsListener
    {
        public const int FailedSendingNotificationId = 1337;

        public static void Initialize()
        {
            CommonConfig.MessengerHub.Subscribe<DocumentUploadStatusChangedMessage>(m =>
            {
                try
                {
                    var i = DocumentsListActivity.CreateIntent(Application.Context, Folder.LocalRootForModule(ModuleType.Documents).SubFolders[0]);

                    var pi = PendingIntent.GetActivity(Application.Context, 0, i, PendingIntentFlags.UpdateCurrent);

                    var title = Application.Context.Resources.GetString(Resource.String.failed_send_document_notification_title);
                    var content = Application.Context.Resources.GetString(Resource.String.failed_send_document_notification_content);
                    var nb = new NotificationCompat.Builder(Application.Context).SetSmallIcon(Resource.Mipmap.ic_notification)
                    .SetColor(ContextCompat.GetColor(Application.Context, Resource.Color.darkerblue))
                    .SetContentIntent(pi).SetContentTitle(title).SetContentText(content)
                    .SetAutoCancel(true)
                    .SetPriority((int)NotificationPriority.High)
                    .SetStyle(new NotificationCompat.BigTextStyle()
                    .BigText(content));

                    if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.NotificationsRingtone))
                        nb.SetSound(Android.Net.Uri.Parse(PlatformConfig.Preferences.NotificationsRingtone));
                    if (PlatformConfig.Preferences.NotificationsVibrate)
                        nb.SetVibrate(new[]
                        {
                        500L,
                        250L,
                        500L
                    });
                    var nm = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                    nm.Notify(FailedSendingNotificationId, nb.Build());
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while sending notification on failed document sending", ex);
                }
            }, m =>
            {
                return m.Change == DocumentUploadStatusChangedMessage.Status.DocumentSentFailed;
            });
        }
    }
}