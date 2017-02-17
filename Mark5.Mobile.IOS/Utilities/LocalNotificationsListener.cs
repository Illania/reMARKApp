//
// Project: Mark5.Mobile.IOS
// File: LocalNotificationsListener.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using UserNotifications

namespace Mark5.Mobile.IOS.Utilities
{
    public static class LocalNotificationsListener
    {
        static public void Initialize()
        {
            Managers.OutgoingDocumentsManager.DocumentSendingFailed += OutgoingDocumentsManager_DocumentSendingFailed; ;
        }

        static void OutgoingDocumentsManager_DocumentSendingFailed(object sender, OutgoingDocumentContainer e)
        {
            var notification = new UNMutableNotificationContent();
        }
    }
}
