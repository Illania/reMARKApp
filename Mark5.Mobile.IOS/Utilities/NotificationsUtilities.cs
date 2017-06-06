//
// Project: Mark5.Mobile.IOS
// File: NotificationsUtilities.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using UserNotifications;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class NotificationsUtilities
    {
        public static Notification Convert(UNNotification notification)
        {
            try
            {
                var n = new Notification();

                var date = notification.Date;
                var request = notification.Request;
                var content = request.Content;
                var userInfo = content.UserInfo;

                var aps = userInfo["aps"] as NSDictionary;
                var alert = aps["alert"] as NSDictionary;
                var title = alert["title"] as NSString;
                var body = alert["body"] as NSString;
                var custom = userInfo["custom"] as NSDictionary;
                var guid = custom["guid"] as NSString;
                var type = custom["type"] as NSString;
                var folderId = custom["folderId"] as NSNumber;
                var objectId = custom["objectId"] as NSNumber;
                var objectType = custom["objectType"] as NSString;

                var reference = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                n.DateTimeTimestamp = reference.AddSeconds(date.SecondsSinceReferenceDate).ConvertDateTimeToTimestampMilliseconds();
                n.Title = title;
                n.Message = body;
                n.Guid = new Guid(guid);
                n.Type = (EventType) Enum.Parse(typeof(EventType), type);
                n.FolderId = folderId.Int32Value;
                n.ObjectId = objectId.Int32Value;
                n.ObjectType = (ObjectType) Enum.Parse(typeof(ObjectType), objectType);

                n.IsRead = false;
                n.IsSilent = false;
                n.RemindOnTimestamp = -1;

                return n;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                return null;
            }
        }
    }
}