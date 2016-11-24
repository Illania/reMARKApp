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

namespace Mark5.Mobile.Droid.Utilities
{

    public static class PushNotificationsConverter
    {

        public static PushNotification ConvertToPushNotification(this RemoteMessage m)
        {
            var pn = new PushNotification
            {
                Data = SerializationUtils.Deserialize<PushNotificationData>(m.Data["data"]),
                Notification = SerializationUtils.Deserialize<PushNotificationNotification>(m.Data["notification"])
            };

            return pn;
        }

        public static Notification ConvertToNotification(this PushNotification pn)
        {
            return null;
        }
    }
}
