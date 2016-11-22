//
// Project: Mark5.Mobile.Droid
// File: GcmListenerService.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.Content;
using Android.OS;
using GCM = Android.Gms.Gcm;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Utilities.PushNotifications
{

    [Service(Exported = false), IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" })]
    public class GcmListenerService : GCM.GcmListenerService
    {

        public override void OnMessageReceived(string from, Bundle data)
        {
            CommonConfig.Logger.Error("NOTIFICATION RECEIVED");

        }
    }
}
