//
// Project: Mark5.Mobile.Droid
// File: DeviceInfoProvider.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.Bluetooth;
using Android.OS;
using Android.Provider;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Utilities
{

    public class DeviceInfoProvider : IDeviceInfoProvider
    {

        public DeviceType GetDeviceType()
        {
            return DeviceType.Android;
        }

        public string GetDeviceId()
        {
            return Settings.Secure.GetString(Application.Context.ContentResolver, Settings.Secure.AndroidId);
        }

        public string GetDeviceName()
        {
            return BluetoothAdapter.DefaultAdapter?.Name ?? Build.Manufacturer + " " + Build.Product + " (" + Build.Model + ")";
        }

        public string GetAppVersionString()
        {
            var ctx = Application.Context;
            var pi = ctx.PackageManager.GetPackageInfo(ctx.PackageName, 0);
            return $"{pi.VersionName} ({pi.VersionCode})";
        }
    }
}

