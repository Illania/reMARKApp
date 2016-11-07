//
// Project: Mark5.Mobile.Droid
// File: Integration.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
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

        public static bool IsRootedMethod1()
        {
            var buildTags = Build.Tags;
            return buildTags != null && buildTags.Contains("test-keys");
        }

        public static bool IsRootedMethod2()
        {
            var paths = new[] { "/system/app/Superuser.apk", "/sbin/su", "/system/bin/su", "/system/xbin/su", "/data/local/xbin/su", "/data/local/bin/su", "/system/sd/xbin/su", "/system/bin/failsafe/su", "/data/local/su", "/su/bin/su" };
            foreach (var path in paths)
            {
                if (new Java.IO.File(path).Exists()) return true;
            }
            return false;
        }

        public static bool IsRootedMethod3()
        {
            Java.Lang.Process process = null;
            try
            {
                process = Java.Lang.Runtime.GetRuntime().Exec(new[] { "/system/xbin/which", "su" });
                var br = new Java.IO.BufferedReader(new Java.IO.InputStreamReader(process.InputStream));
                if (br.ReadLine() != null) return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                process?.Dispose();
            }
        }

        public static void ClearDataAndStop()
        {
            var am = Application.Context.GetSystemService(Context.ActivityService) as ActivityManager;
            am.ClearApplicationUserData();
        }
    }
}

