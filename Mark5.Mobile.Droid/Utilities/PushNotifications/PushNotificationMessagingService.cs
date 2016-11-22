//
// Project: Mark5.Mobile.Droid
// File: PushNotificationMessagingService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.Content;
using Firebase.Messaging;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Utilities.PushNotifications
{

    [Service(Name = "com.nordic_it.mark5.android.PushNotificationMessagingService"), IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PushNotificationMessagingService : FirebaseMessagingService
    {

        public override void OnMessageReceived(RemoteMessage message)
        {
            CommonConfig.Logger.Info("Notification received");
        }
    }
}
