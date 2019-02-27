using System;
using Android.OS;
using Firebase.Messaging;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class PushNotificationsConverter
    {
        public static Notification ConvertToNotification(this RemoteMessage m)
        {
            return m.ConvertToPushNotification().ConvertToNotification();
        }

        public static PushNotification ConvertToPushNotification(this RemoteMessage message)
        {
            if (message.Data.ContainsKey("data"))
            {
                return new PushNotification
                {
                    Data = Serializer.Deserialize<PushNotificationData>(message.Data["data"]),
                    Notification = Serializer.Deserialize<PushNotificationNotification>(message.Data["notification"])
                };
            }
            else
            {
                var data = new PushNotificationData
                {
                    Silent = message.Data.ContainsKey("silent") ? int.Parse(message.Data["silent"]) : 0,
                    Guid = message.Data.ContainsKey("guid") ? Guid.Parse(message.Data["guid"]) : Guid.Empty,
                    Type = message.Data.ContainsKey("type") ? (EventType)Enum.Parse(typeof(EventType), message.Data["type"]) : EventType.None,
                    ObjectId = message.Data.ContainsKey("objectId") ? int.Parse(message.Data["objectId"]) : 0,
                    FolderId = message.Data.ContainsKey("folderId") ? int.Parse(message.Data["folderId"]) : 0,
                    ObjectType = message.Data.ContainsKey("objectType") ? (ObjectType)Enum.Parse(typeof(ObjectType), message.Data["objectType"]) : ObjectType.None,
                    RemindOn = message.Data.ContainsKey("remindOn") ? message.Data["remindOn"] : null,
                };

                var notification = new PushNotificationNotification
                {
                    Title = message.Data.ContainsKey("title") ? message.Data["title"] : null,
                    Body = message.Data.ContainsKey("body") ? message.Data["body"] : null,
                    Icon = message.Data.ContainsKey("icon") ? message.Data["icon"] : null,
                };

                return new PushNotification
                {
                    Data = data,
                    Notification = notification,
                };
            }
        }

        public static Notification ExtractNotification(Bundle extra)
        {
            var data = new PushNotificationData
            {
                Silent = extra.ContainsKey("silent") ? int.Parse(extra.GetString("silent")) : 0,
                Guid = extra.ContainsKey("guid") ? Guid.Parse(extra.GetString("guid")) : Guid.Empty,
                Type = extra.ContainsKey("type") ? (EventType)Enum.Parse(typeof(EventType), extra.GetString("type")) : EventType.None,
                ObjectId = extra.ContainsKey("objectId") ? int.Parse(extra.GetString("objectId")) : 0,
                FolderId = extra.ContainsKey("folderId") ? int.Parse(extra.GetString("folderId")) : 0,
                ObjectType = extra.ContainsKey("objectType") ? (ObjectType)Enum.Parse(typeof(ObjectType), extra.GetString("objectType")) : ObjectType.None,
                RemindOn = extra.ContainsKey("remindOn") ? extra.GetString("remindOn") : null,
            };

            var notification = new PushNotificationNotification
            {
                Title = extra.ContainsKey("title") ? extra.GetString("title") : null,
                Body = extra.ContainsKey("body") ? extra.GetString("body") : null,
                Icon = extra.ContainsKey("icon") ? extra.GetString("icon") : null,
            };

            var pn = new PushNotification
            {
                Data = data,
                Notification = notification,
            };

            return pn.ConvertToNotification();
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