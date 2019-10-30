using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Firebase;
using Firebase.Iid;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class SystemReportCollector
    {
        public static Intent CreateShareReportComposeDocumentActivityIntent(Context context, string preconfiguredContent)
        {
            return ComposeDocumentActivity.CreateShareReportIntent(context, "reMARK Android System Report", preconfiguredContent);
        }

        public static Intent CreateShareFeedbackComposeDocumentActivityIntent(Context context, string preconfiguredContent)
        {
            return ComposeDocumentActivity.CreateShareReportIntent(context, "reMARK Android Feedback", preconfiguredContent);
        }

        public static Intent CreateShareReportIntent(Context context, string report)
        {
            var sendIntent = new Intent();
            sendIntent.SetAction(Intent.ActionSend);
            sendIntent.PutExtra(Intent.ExtraEmail, new[] { "appfeedback@nordic-it.com" });
            sendIntent.PutExtra(Intent.ExtraSubject, "reMARK Android System Report");
            sendIntent.PutExtra(Intent.ExtraText, report);
            sendIntent.SetType("text/plain");
            return Intent.CreateChooser(sendIntent, context.GetText(Resource.String.share));
        }

        public static Intent CreateShareFeedbackIntent(string report)
        {
            var sendIntent = new Intent();
            sendIntent.SetAction(Intent.ActionSendto);
            sendIntent.SetData(Android.Net.Uri.Parse("mailto:appfeedback@nordic-it.com?subject=reMARK%20Android%20Feedback&body=" + report));
            return sendIntent;
        }

        public static string CreateFullReport()
        {
            var sb = new StringBuilder();

            sb.Append(CreateSystemInfoReport());
            sb.AppendLine();
            sb.Append(CreateServerInfoReport());
            sb.AppendLine();
            sb.Append(CreateLogCatReport());

            return sb.ToString();
        }

        public static string CreateSystemInfoReport()
        {
            var sb = new StringBuilder();

            sb.AppendLine("===== General =====");
            sb.AppendLine("Platform: Android");
            var pi = Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0);
            sb.AppendLine("Version: " + pi.VersionName + " (" + pi.VersionCode + ")");
            sb.AppendLine("UTC Date: " + DateTime.UtcNow);
            sb.AppendLine("Local Date: " + DateTime.Now);
            sb.AppendLine();

            sb.AppendLine("===== Device information =====");
            sb.AppendLine("Manufacturer: " + Build.Manufacturer);
            sb.AppendLine("Product: " + Build.Product);
            sb.AppendLine("Brand: " + Build.Brand);
            sb.AppendLine("Model: " + Build.Model);
            sb.AppendLine("Device: " + Build.Device);
            sb.AppendLine("Installation ID: " + CommonConfig.DeviceInfoProvider.GetDeviceId());
            sb.AppendLine("Display: " + Build.Display);
            sb.AppendLine("Version.BaseOs: " + Build.VERSION.BaseOs);
            sb.AppendLine("Version.Codename: " + Build.VERSION.Codename);
            sb.AppendLine("Version.Incremental: " + Build.VERSION.Incremental);
            sb.AppendLine("Version.PreviewSdkInt: " + Build.VERSION.PreviewSdkInt);
            sb.AppendLine("Version.Release: " + Build.VERSION.Release);
            sb.AppendLine("Version.Sdk: " + Build.VERSION.Sdk);
            sb.AppendLine("Version.SdkInt: " + Build.VERSION.SdkInt);
            sb.AppendLine("Version.SecurityPatch: " + Build.VERSION.SecurityPatch);
            sb.AppendLine("Root: " + Integration.IsRootedMethod1() + "," + Integration.IsRootedMethod2() + "," + Integration.IsRootedMethod3());
            sb.AppendLine("Firebase initialized: " + (FirebaseApp.Instance == null ? "false" : "true"));
            sb.AppendLine();

            sb.AppendLine("===== Connection information =====");
            sb.AppendLine("Username: " + Managers.ActiveConnectionInfo?.Username);
            sb.AppendLine("Hostname: " + Managers.ActiveConnectionInfo?.Hostname);
            sb.AppendLine("Port: " + Managers.ActiveConnectionInfo?.Port);
            sb.AppendLine("SSL: " + Managers.ActiveConnectionInfo?.SslMode);
            sb.AppendLine("Friendly device name: " + Managers.ActiveConnectionInfo?.FriendlyDeviceName);
            sb.AppendLine("Installation ID: " + Managers.ActiveConnectionInfo?.InstallationId);
            sb.AppendLine("Firebase Instance ID: " + FirebaseInstanceId.Instance?.Token);
            sb.AppendLine();

            sb.AppendLine("===== Preferences =====");
            foreach (var kv in PlatformConfig.Preferences.All)
                sb.AppendLine(kv.Key + ": " + kv.Value);

            sb.AppendLine();

            sb.AppendLine("===== Memory information =====");
            var am = Application.Context.GetSystemService(Context.ActivityService) as ActivityManager;
            var mi = new ActivityManager.MemoryInfo();
            am.GetMemoryInfo(mi);
            sb.AppendLine("Available memory KB: " + mi.AvailMem / 1024);
            sb.AppendLine("Total memory KB: " + mi.TotalMem / 1024);
            sb.AppendLine("Low memory: " + mi.LowMemory);
            sb.AppendLine("[MONO] Total memory: " + GC.GetTotalMemory(false) / 1024);
            sb.AppendLine("[MONO] Total memory after GC: " + GC.GetTotalMemory(true) / 1024);
            sb.AppendLine();

            sb.AppendLine("===== Network information =====");
            var cm = Application.Context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;
            var activeNetwork = cm.ActiveNetwork;

            if (activeNetwork != null)
            {
                var networkInfo = cm.GetNetworkInfo(activeNetwork);
                var networkCapabilities = cm.GetNetworkCapabilities(activeNetwork);
                sb.AppendLine($"Network {activeNetwork}:");
                sb.AppendLine("  IsReachable: " + CommonConfig.Reachability?.IsReachable);
                sb.AppendLine("  IsCellular: " + networkCapabilities.HasTransport(TransportType.Cellular));
                sb.AppendLine("  IsVpn: " + networkCapabilities.HasTransport(TransportType.Vpn));
                sb.AppendLine("  IsWifi: " + networkCapabilities.HasTransport(TransportType.Wifi));
                sb.AppendLine("  IsWifiAware: " + networkCapabilities.HasTransport(TransportType.WifiAware));
                sb.AppendLine("  NotRoaming:" + networkCapabilities.HasCapability(NetCapability.NotRoaming));
                sb.AppendLine("  Subtype: " + networkInfo.Subtype);
                sb.AppendLine("  Connected:" + networkInfo.IsConnected);
                sb.AppendLine("  Extra info:" + networkInfo.ExtraInfo);
            }
            else
            {
                sb.AppendLine("Active network is null");
            }

            sb.AppendLine();

            sb.AppendLine("===== Properties =====");
            var props = Java.Lang.JavaSystem.Properties;
            foreach (var propName in props.StringPropertyNames())
                sb.AppendLine(propName + ": " + props.GetProperty(propName));

            return sb.ToString();
        }

        public static string CreateServerInfoReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("===== Server information =====");
            sb.AppendLine(Serializer.SerializeSelectively(ServerConfig.SystemSettings, new[] { (typeof(ContactsModuleInfo), nameof(ContactsModuleInfo.Countries)) }));
            return sb.ToString();
        }

        public static string CreateLogCatReport()
        {
            var sb = new Java.Lang.StringBuilder();
            sb.Append("===== logcat =====");

            try
            {
                var r = Java.Lang.Runtime.GetRuntime()
                    .Exec(new[]
                    {
                        "logcat",
                        "-d",
                        "Mono:I",
                        "MARK5:V",
                        "*:S"
                    });
                string line = null;
                using (var isr = new Java.IO.InputStreamReader(r.InputStream))
                using (var br = new Java.IO.BufferedReader(isr))
                {
                    while ((line = br.ReadLine()) != null)
                    {
                        sb.Append(Java.Lang.JavaSystem.GetProperty("line.separator"));
                        sb.Append(line);
                    }
                }
            }
            catch (Java.Lang.Exception e)
            {
                sb.Append(Java.Lang.JavaSystem.GetProperty("line.separator"));
                sb.Append("!!!!! logcat unavailable. !!!!!");
                sb.Append(Java.Lang.JavaSystem.GetProperty("line.separator"));
                sb.Append(e.Message);
            }

            sb.Append(Java.Lang.JavaSystem.GetProperty("line.separator"));

            return sb.ToString();
        }

        public static string CreateExceptionReport(Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine("===== Exception =====");
            sb.AppendLine("Exception Type: " + ex.GetType());
            sb.AppendLine();
            sb.AppendLine("Stack trace: " + ex.StackTrace);

            return sb.ToString();
        }

        public static string CreateFailedDocumentReport(Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine("===== Document Failed Report =====");
            sb.Append(CreateSystemInfoReport());
            sb.AppendLine();
            sb.Append(CreateServerInfoReport());
            sb.AppendLine();
            sb.Append(CreateExceptionReport(ex));

            return sb.ToString();
        }
    }
}