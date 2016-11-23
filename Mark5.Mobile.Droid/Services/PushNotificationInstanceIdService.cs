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
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Services
{

    [Service, IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class PushNotificationInstanceIdService : FirebaseInstanceIdService
    {

        public override void OnTokenRefresh()
        {
            var token = FirebaseInstanceId.Instance.Token;

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug($"Received Firebase token: {token}");

            var oldToken = PlatformConfig.Preferences.PushNotificationToken;
            PlatformConfig.Preferences.PushNotificationToken = token;

            if (Managers.ActiveConnectionInfo != null)
            {
                CommonConfig.Logger.Info($"Sending Firebase token to service...");

                if (!string.IsNullOrWhiteSpace(oldToken) && oldToken != token)
                {
                    CommonConfig.Logger.Info($"New Firebase token is different, so try to unsubscribe old one...");

                    Managers.NotificationsManager.UnSubscribe(DeviceType.Android, token).FireAndForget();
                }

                Managers.NotificationsManager.Subscribe(DeviceType.Android, token).FireAndForget();
            }
        }
    }
}