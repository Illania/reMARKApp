//
// Project: Mark5.Mobile.Droid
// File: InstanceIdListenerService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
/*using Android.App;
using Android.Content;
using Android.Gms.Gcm.Iid;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Utilities.PushNotifications
{

    [Service(Exported = false), IntentFilter(new[] { "com.google.android.gms.iid.InstanceID" })]
    public class InstanceIdListenerService : InstanceIDListenerService
    {

        public override void OnTokenRefresh()
        {
            CommonConfig.Logger.Info("Will refresh token...");

            var intent = new Intent(this, typeof(RegistrationIntentService));
            StartService(intent);
        }
    }
}
*/