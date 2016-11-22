//
// Project: Mark5.Mobile.Droid
// File: PushNotificationInstanceIdService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.Content;
using Firebase.Iid;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Utilities.PushNotifications
{

    [Service(Exported = false), IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class PushNotificationInstanceIdService : FirebaseInstanceIdService
    {

        public override void OnTokenRefresh()
        {
            CommonConfig.Logger.Info("Will refresh token...");

            PlatformConfig.Preferences.PushNotificationToken = FirebaseInstanceId.Instance.Token;
        }
    }
}