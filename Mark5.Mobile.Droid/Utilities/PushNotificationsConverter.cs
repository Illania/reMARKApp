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
            if (m.Data.ContainsKey("data"))
            {
                return new PushNotification
                {
                    Data = Serializer.Deserialize<PushNotificationData>(m.Data["data"]),
                    Notification = Serializer.Deserialize<PushNotificationNotification>(m.Data["notification"])
                };
            }
            else
            {
                var data = new PushNotificationData
                {
                    Silent = m.Data.ContainsKey("silent") ? int.Parse(m.Data["silent"]) : 0,
                    Guid = m.Data.ContainsKey("guid") ? Guid.Parse(m.Data["guid"]) : Guid.Empty,
                    Type = m.Data.ContainsKey("type") ? (EventType)Enum.Parse(typeof(EventType), m.Data["type"]) : EventType.None,
                    ObjectId = m.Data.ContainsKey("objectId") ? int.Parse(m.Data["objectId"]) : 0,
                    FolderId = m.Data.ContainsKey("folderId") ? int.Parse(m.Data["folderId"]) : 0,
                    ObjectType = m.Data.ContainsKey("objectType") ? (ObjectType)Enum.Parse(typeof(ObjectType), m.Data["objectType"]) : ObjectType.None,
                    RemindOn = m.Data.ContainsKey("remindOn") ? m.Data["remindOn"] : null,
                };

                var notification = new PushNotificationNotification
                {
                    Title = m.Data.ContainsKey("title") ? m.Data["title"] : null,
                    Body = m.Data.ContainsKey("body") ? m.Data["body"] : null,
                    Icon = m.Data.ContainsKey("icon") ? m.Data["icon"] : null,
                };

                return new PushNotification
                {
                    Data = data,
                    Notification = notification,
                };
            }
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