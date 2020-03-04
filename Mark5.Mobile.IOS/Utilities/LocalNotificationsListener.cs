using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Ui.Common;
using UserNotifications;
using Mark5.Mobile.Common.Model.HubMessages;
using Foundation;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class LocalNotificationsListener
    {
        public const string DocumentFailedToSendIdentifier = "DocumentFailedToSend";

        public static void Initialize()
        {
            CommonConfig.MessengerHub.Subscribe<DocumentUploadStatusChangedMessage>(m =>
            {
                NSOperationQueue.MainQueue.InvokeOnMainThread(() =>
                {
                    var notificatioContent = new UNMutableNotificationContent();

                    var titleString = Localization.GetString("failed_send_document_notification_title");
                    var contentString = Localization.GetString("failed_send_document_notification_content");

                    notificatioContent.Title = titleString;
                    notificatioContent.Body = contentString;

                    var request = UNNotificationRequest.FromIdentifier(DocumentFailedToSendIdentifier, notificatioContent, null);
                    UNUserNotificationCenter.Current.AddNotificationRequest(request, null);
                });
            }, m =>
            {
                return m.Change == DocumentUploadStatusChangedMessage.Status.DocumentSentFailed;
            });
        }
    }
}