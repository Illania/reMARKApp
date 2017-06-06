using System;
using System.Globalization;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mark5.Mobile.Droid.Model
{
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

        public bool IsSilent => Silent > 0;

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
                if (string.IsNullOrWhiteSpace(RemindOn))
                    return 0;

                return DateTime.ParseExact(RemindOn, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture).ConvertDateTimeToTimestampMilliseconds();
            }
        }
    }
}