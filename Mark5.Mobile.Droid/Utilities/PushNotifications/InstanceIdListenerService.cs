//
// Project: Mark5.Mobile.Droid
// File: InstanceIdListenerService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Utilities.PushNotifications
{
    [Android.App.Service(Exported = false), Android.App.IntentFilter(new[] { "com.google.android.gms.iid.InstanceID" })]
    public class InstanceIdListenerService : Android.Gms.Gcm.Iid.InstanceIDListenerService
    {
        public override void OnTokenRefresh()
        {
            CommonConfig.Logger.Info("Token requires refresh.");

            var intent = new Android.Content.Intent(this, typeof(RegistrationIntentService));
            StartService(intent);
        }
    }
}
