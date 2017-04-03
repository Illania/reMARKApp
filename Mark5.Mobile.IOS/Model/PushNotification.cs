//
// Project: Mark5.Mobile.IOS
// File: PushNotification.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Globalization;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mark5.Mobile.IOS.Model
{

    [JsonObject(MemberSerialization.OptIn)]
    public class PushNotification
    {

        [JsonProperty("aps")]
        public Aps Aps { get; set; }

        [JsonProperty("custom")]
        public Custom Custom { get; set; }
    }


    [JsonObject(MemberSerialization.OptIn)]
    public class Aps
    {

        [JsonProperty("content-available")]
        public int ContenAvailable { get; set; }

        public bool IsContenAvailable
        {
            get
            {
                return ContenAvailable > 0;
            }
        }

        [JsonProperty("alert")]
        public Alert Alert { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Alert
    {

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Custom
    {

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
