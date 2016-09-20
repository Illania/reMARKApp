//
// Project: Mark5.Mobile.Droid
// File: RegistrationIntentService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Utilities.PushNotifications
{
    [Android.App.Service(Exported = false)]
    class RegistrationIntentService : Android.App.IntentService
    {
        static readonly object handleIntentLock = new object();
        const string SenderId = "887732996602";

        protected override void OnHandleIntent(Android.Content.Intent intent)
        {
            try
            {
                CommonConfig.Logger.Info("Requesting token...");

                lock (handleIntentLock)
                {
                    var instanceId = Android.Gms.Gcm.Iid.InstanceID.GetInstance(Android.App.Application.Context);
                    var token = instanceId.GetToken(SenderId, Android.Gms.Gcm.GoogleCloudMessaging.InstanceIdScope, null);

                    CommonConfig.Logger.Info(string.Format("Token received. [token={0}]", token));

                    PlatformConfig.Preferences.PushNotificationToken = token;
                }
            }
            catch (Exception e)
            {
                CommonConfig.Logger.Error("Request token failed.", e);

                return;
            }
        }
    }
}
