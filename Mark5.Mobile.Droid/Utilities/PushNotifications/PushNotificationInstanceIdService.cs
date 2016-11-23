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

    [Service, IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class PushNotificationInstanceIdService : FirebaseInstanceIdService
    {

        public override void OnTokenRefresh()
        {
            var token = FirebaseInstanceId.Instance.Token;

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug($"Received Firebase token: {token}");

            PlatformConfig.Preferences.PushNotificationToken = token;
        }
    }
}