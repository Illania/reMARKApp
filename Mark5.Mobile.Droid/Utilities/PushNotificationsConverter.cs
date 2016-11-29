//
// Project: Mark5.Mobile.Droid
// File: PushNotificationsConverter.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Firebase.Messaging;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.Common.Model;
using System;

namespace Mark5.Mobile.Droid.Utilities
{

    public static class PushNotificationsConverter
    {

        public static Notification ConvertToNotification(this RemoteMessage m)
        {
            return m.ConvertToPushNotification().ConvertToNotification();
        }

        public static PushNotification ConvertToPushNotification(this RemoteMessage m)
        {
            return new PushNotification
            {
                Data = SerializationUtils.Deserialize<PushNotificationData>(m.Data["data"]),
                Notification = SerializationUtils.Deserialize<PushNotificationNotification>(m.Data["notification"])
            };
        }

        public static Notification ConvertToNotification(this PushNotification pn)
        {
            return new Notification
            {
                Guid = pn.Data.Guid,
                Title = pn.Notification.Title,
                Message = pn.Notification.Body,
                Type = pn.Data.Type,
                FolderId = pn.Data.FolderId,
                ObjectType = pn.Data.ObjectType,
                ObjectId = pn.Data.ObjectId,
                IsSilent = pn.Data.IsSilent,
                DateTimeTimestamp = DateTime.UtcNow.ConvertDateTimeToTimestampMilliseconds(),
                RemindOnTimestamp = pn.Data.RemindOnTimestamp
            };
        }
    }
}
