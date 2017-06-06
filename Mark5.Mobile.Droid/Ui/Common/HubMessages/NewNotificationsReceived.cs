//
// Project: Mark5.Mobile.Droid
// File: NewNotificationsReceived.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{
    public class NewNotificationsReceived : TinyMessageBase
    {
        public NewNotificationsReceived(object sender)
            : base(sender)
        {
        }
    }
}