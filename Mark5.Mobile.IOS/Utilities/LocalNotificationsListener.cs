//
// Project: Mark5.Mobile.IOS
// File: LocalNotificationsListener.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UserNotifications;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class LocalNotificationsListener
    {
        public const string FailedSendingIdentifier = "FailedSendingIdentifier";

        static public void Initialize()
        {
            Managers.OutgoingDocumentsManager.DocumentSendingFailed += OutgoingDocumentsManager_DocumentSendingFailed;
        }

        static void OutgoingDocumentsManager_DocumentSendingFailed(object sender, OutgoingDocumentContainer e)
        {
            //TODO Questions for Bartosz:
            //Should we open the outgoing document list when the user clicks on the notification or do nothing? I vote for nothing
            var notificatioContent = new UNMutableNotificationContent();

            var titleString = Localization.GetString("failed_send_document_notification_title");
            var contentString = Localization.GetString("failed_send_document_notification_content");

            notificatioContent.Title = titleString;
            notificatioContent.Body = contentString;

            var request = UNNotificationRequest.FromIdentifier(FailedSendingIdentifier, notificatioContent, null);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, err =>
            {
                if (err != null)
                {
                    CommonConfig.Logger.Error($"Error while sending notification for failed send document: {err}");
                }
            });

        }
    }
}
