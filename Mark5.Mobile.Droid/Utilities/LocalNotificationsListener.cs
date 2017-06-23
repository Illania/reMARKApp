using System;
using Mark5.Mobile.Common.Managers;
using Android.Support.V4.App;
using Android.OS;
using Android.Support.V4.Content;
using Android.App;
using Android.Content;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class LocalNotificationsListener
    {
        public const int FailedSendingNotificationId = 11;

        public static void Initialize()
        {
            Managers.OutgoingDocumentsManager.DocumentSendingFailed += OutgoingDocumentsManager_DocumentSendingFailed;
        }

        static void OutgoingDocumentsManager_DocumentSendingFailed(object sender, DocumentToUploadContainer e)
        {
            try
            {
                var i = new Intent(Application.Context, typeof(DocumentsListActivity));
                i.PutExtra(DocumentsListActivity.FolderIntentKey, SerializationUtils.Serialize(Folder.DocumentsOutgoingFolder));
                var pi = PendingIntent.GetActivity(Application.Context, 0, i, PendingIntentFlags.UpdateCurrent);

                var title = Application.Context.Resources.GetString(Resource.String.failed_send_document_notification_title);
                var content = Application.Context.Resources.GetString(Resource.String.failed_send_document_notification_content);
                var nb = new NotificationCompat.Builder(Application.Context).SetSmallIcon(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ? Resource.Mipmap.ic_icon_lollipop : Resource.Mipmap.ic_icon).SetColor(ContextCompat.GetColor(Application.Context, Resource.Color.darkerblue)).SetContentIntent(pi).SetContentTitle(title).SetContentText(content).SetAutoCancel(true).SetPriority((int) NotificationPriority.High).SetStyle(new NotificationCompat.BigTextStyle().BigText(content));

                if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.NotificationsRingtone))
                    nb.SetSound(Android.Net.Uri.Parse(PlatformConfig.Preferences.NotificationsRingtone));
                if (PlatformConfig.Preferences.NotificationsVibrate)
                    nb.SetVibrate(new[]
                    {
                        500L,
                        250L,
                        500L
                    });
                var nm = (NotificationManager) Application.Context.GetSystemService(Context.NotificationService);
                nm.Notify(FailedSendingNotificationId, nb.Build());
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while sending notification on failed document sending", ex);
            }
        }
    }
}