//
// Project: Mark5.Mobile.Droid
// File: RegistrationIntentService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
/*using System;
using Android.App;
using Android.Content;
using Android.Gms.Gcm;
using Android.Gms.Gcm.Iid;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Utilities.PushNotifications
{

    [Service(Exported = false)]
    class RegistrationIntentService : IntentService
    {

        const string SenderId = "887732996602";

        static readonly object handleIntentLock = new object();

        protected override void OnHandleIntent(Intent intent)
        {
            try
            {
                CommonConfig.Logger.Info("Requesting token...");

                lock (handleIntentLock)
                {
                    var instanceId = InstanceID.GetInstance(Application);
                    var token = instanceId.GetToken(SenderId, GoogleCloudMessaging.InstanceIdScope, null);

                    CommonConfig.Logger.Info($"Token received [token={token}]");

                    PlatformConfig.Preferences.PushNotificationToken = token;
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Request token failed", ex);
            }
        }
    }
}*/
