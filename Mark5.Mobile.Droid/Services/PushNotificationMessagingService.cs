//
// Project: Mark5.Mobile.Droid
// File: PushNotificationMessagingService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.App;
using Android.Content;
using Firebase.Messaging;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Utilities.Services
{

    [Service, IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PushNotificationMessagingService : FirebaseMessagingService
    {

        public override void OnMessageReceived(RemoteMessage message)
        {
            CommonConfig.Logger.Info("Notification received");

            try
            {
                var pn = message.ConvertToPushNotification();

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not process notification", ex);
            }
        }
    }
}
