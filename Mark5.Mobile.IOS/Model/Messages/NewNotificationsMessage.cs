//
// Project: Mark5.Mobile.IOS
// File: NewNotificationsMessage.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.Messages
{

    public class NewNotificationsMessage : TinyMessageBase //TODO this message is not together with the others in UI/COMMON/HUBMESSAGES
    {

        public NewNotificationsMessage(object sender)
            : base(sender)
        {
        }
    }
}
