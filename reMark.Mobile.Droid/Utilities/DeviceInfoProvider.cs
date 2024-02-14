using Android.App;
using Android.OS;
using Android.Provider;
using AndroidX.Core.Content.PM;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using Application = Android.App.Application;
using DeviceType = reMark.Mobile.Common.Model.DeviceType;

namespace reMark.Mobile.Droid.Utilities
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
            return $"{Build.Manufacturer} {Build.Model}";
        }

        public string GetAppVersionString()
        {
            var ctx = Application.Context;
            var pi = ctx.PackageManager.GetPackageInfo(ctx.PackageName, 0);
            var longVersionCode = PackageInfoCompat.GetLongVersionCode(pi);

            return $"{pi.VersionName} ({longVersionCode})";
        }
    }
}