//
// Project: Mark5.Mobile.Droid
// File: ReachabilityBroadcastReceiver.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Net;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Services
{

    public class ReachabilityBroadcastReceiver : BroadcastReceiver
    {

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != ConnectivityManager.ConnectivityAction)
            {
                return;
            }

            GoAsync();
            CommonConfig.ReachabilityService.Refresh().FireAndForget();
        }
    }
}

