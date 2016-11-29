//
// Project: Mark5.Mobile.Common
// File: NotificationSettings.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{
    public class NotificationSettings
    {

        public string SoundName { get; set; } = "default";

        public bool CalendarNotificationsEnabled { get; set; }
    }
}

