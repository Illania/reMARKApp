using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model;
using UserNotifications;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class PushNotificationsConverter
    {
        public static Notification ConvertToNotification(this NSDictionary dict)
        {
            return dict.ConvertToPushNotification().ConvertToNotification();
        }

        public static Notification ConvertToNotification(this UNNotification n)
        {
            return n.ConvertToPushNotification().ConvertToNotification();
        }

        public static PushNotification ConvertToPushNotification(this UNNotification n)
        {
            var userInfoDict = n.Request.Content.UserInfo;
            return ConvertToPushNotification(userInfoDict);
        }

        public static PushNotification ConvertToPushNotification(this NSDictionary userInfoDict)
        {
            NSError _error;
            var userInfoData = NSJsonSerialization.Serialize(userInfoDict, NSJsonWritingOptions.PrettyPrinted, out _error);
            var userInfoJson = new NSString(userInfoData, NSStringEncoding.UTF8);

            return Serializer.Deserialize<PushNotification>(userInfoJson);
        }

        public static Notification ConvertToNotification(this PushNotification pn)
        {
            return new Notification
            {
                Guid = pn.Custom.Guid,
                Title = pn.Aps.Alert.Title,
                Message = pn.Aps.Alert.Body,
                Type = pn.Custom.Type,
                FolderId = pn.Custom.FolderId,
                ObjectType = pn.Custom.ObjectType,
                ObjectId = pn.Custom.ObjectId,
                IsSilent = pn.Aps.IsContenAvailable,
                DateTimeTimestamp = DateTime.UtcNow.ConvertDateTimeToTimestampMilliseconds(),
                RemindOnTimestamp = pn.Custom.RemindOnTimestamp
            };
        }
    }
}