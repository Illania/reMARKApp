//
// Project: Mark5.Mobile.IOS
// File: NewNotificationsMessage.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class NewNotificationsMessage : TinyMessageBase
    {

        public NewNotificationsMessage(object sender)
            : base(sender)
        {
        }
    }
}
