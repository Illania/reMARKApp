//
// Project: Mark5.Mobile.Droid
// File: PushNotificationMessagingService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Firebase.Messaging;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mark5.Mobile.Droid.Utilities.Services
{

    [Service, IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PushNotificationMessagingService : FirebaseMessagingService
    {

        public override void OnMessageReceived(RemoteMessage message)
        {
            CommonConfig.Logger.Info("Notification received");

            var pn = ConvertToPushNotification(message);
        }

        PushNotification ConvertToPushNotification(RemoteMessage message)
        {
            var pn = new PushNotification
            {
                Data = SerializationUtils.Deserialize<PushNotificationData>(message.Data["data"]),
                Notification = SerializationUtils.Deserialize<PushNotificationNotification>(message.Data["notification"])
            };

            return pn;
        }

        Common.Model.Notification ConvertToNotification(PushNotification pushnotification)
        {
            return new Common.Model.Notification();
        }

        #region Model

        public class PushNotification
        {

            public PushNotificationNotification Notification { get; set; }

            public PushNotificationData Data { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class PushNotificationNotification
        {

            [JsonProperty("icon")]
            public string Icon { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class PushNotificationData
        {

            [JsonProperty("silent")]
            public int Silent { get; set; }

            public bool IsSilent
            {
                get
                {
                    return Silent > 0;
                }
            }

            [JsonProperty("guid")]
            public Guid Guid { get; set; }

            [JsonProperty("type")]
            [JsonConverter(typeof(StringEnumConverter))]
            public EventType Type { get; set; }

            [JsonProperty("objectId")]
            public int ObjectId { get; set; }

            [JsonProperty("folderId")]
            public int FolderId { get; set; }

            [JsonProperty("objectType")]
            [JsonConverter(typeof(StringEnumConverter))]
            public ObjectType ObjectType { get; set; }

            [JsonProperty("remindOn")]
            public string RemindOn { get; set; }

            public long RemindOnTimestamp
            {
                get
                {
                    return DateTime.ParseExact(RemindOn, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture).ConvertDateTimeToTimestampMilliseconds();
                }
            }
        }

        #endregion

    }
}
