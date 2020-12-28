using System;
using Android.OS;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class PushNotificationsConverter
    {
      
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
                Body = extra.ContainsKey("body") ? extra.GetString("body") : null
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