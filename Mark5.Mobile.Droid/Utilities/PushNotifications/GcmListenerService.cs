//
// Project: Mark5.Mobile.Droid
// File: GcmListenerService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

namespace Mark5.Mobile.Droid.Utilities.PushNotifications
{
    [Android.App.Service(Exported = false), Android.App.IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" })]
    public class GcmListenerService : Android.Gms.Gcm.GcmListenerService
    {
        public override void OnMessageReceived(string from, Android.OS.Bundle data)
        {
            //TODO need to complete
        }
    }
}
