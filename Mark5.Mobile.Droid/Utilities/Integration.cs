//
// Project: Mark5.Mobile.Droid
// File: Integration.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.Content;
using Android.Net;
using Android.Support.V4.Net;

namespace Mark5.Mobile.Droid.Utilities
{

    public static class Integration
    {

        public static bool IsConnectedToMeteredConnection()
        {
            var ctx = Application.Context;
            var cm = (ConnectivityManager)ctx.GetSystemService(Context.ConnectivityService);
            return ConnectivityManagerCompat.IsActiveNetworkMetered(cm);
        }

        public static void ClearDataAndStop()
        {
            var am = Application.Context.GetSystemService(Context.ActivityService) as ActivityManager;
            am.ClearApplicationUserData();
        }
    }
}

