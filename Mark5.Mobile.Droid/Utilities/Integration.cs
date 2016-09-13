//
// Project: Mark5.Mobile.Droid
// File: Integration.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.Content;

namespace Mark5.Mobile.Droid.Utilities
{

    public static class Integration
    {

        public static void ClearDataAndStop()
        {
            var am = Application.Context.GetSystemService(Context.ActivityService) as ActivityManager;
            am.ClearApplicationUserData();
        }
    }
}

