//
// Project: Mark5.Mobile.Droid
// File: SystemReportCollector.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Text;
using Android.OS;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class SystemReportCollector
    {

        public static string CreateFullReport()
        {
            var sb = new StringBuilder();

            sb.Append(CreateSystemInfoReport());
            sb.AppendLine();
            sb.Append(CreateLogCatReport());

            return sb.ToString();
        }

        public static string CreateSystemInfoReport()
        {
            var sb = new StringBuilder();

            sb.AppendLine("===== Connection information =====");
            sb.AppendLine();

            sb.AppendLine("===== Server information =====");
            sb.AppendLine();

            sb.AppendLine("===== Preferences =====");
            foreach (var kv in PlatformConfig.Preferences.All)
            {
                sb.AppendLine(kv.Key + ": " + kv.Value);
            }
            sb.AppendLine();

            sb.AppendLine("===== Device information =====");
            sb.AppendLine("Manufacturer: " + Build.Manufacturer);
            sb.AppendLine("Product: " + Build.Product);
            sb.AppendLine("Brand: " + Build.Brand);
            sb.AppendLine("Model: " + Build.Model);
            sb.AppendLine("Device: " + Build.Device);
            sb.AppendLine("Display: " + Build.Display);
            sb.AppendLine("Version.BaseOs: " + Build.VERSION.BaseOs);
            sb.AppendLine("Version.Codename: " + Build.VERSION.Codename);
            sb.AppendLine("Version.Incremental: " + Build.VERSION.Incremental);
            sb.AppendLine("Version.PreviewSdkInt: " + Build.VERSION.PreviewSdkInt);
            sb.AppendLine("Version.Release: " + Build.VERSION.Release);
            sb.AppendLine("Version.Sdk: " + Build.VERSION.Sdk);
            sb.AppendLine("Version.SdkInt: " + Build.VERSION.SdkInt);
            sb.AppendLine("Version.SecurityPatch: " + Build.VERSION.SecurityPatch);
            sb.AppendLine();

            sb.AppendLine("===== Memory information =====");
            sb.AppendLine();

            sb.AppendLine("===== Network information =====");
            sb.AppendLine();

            sb.AppendLine("===== Properties =====");
            var props = Java.Lang.JavaSystem.Properties;
            foreach (var propName in props.StringPropertyNames())
            {
                sb.AppendLine(propName + ": " + props.GetProperty(propName));
            }

            return sb.ToString();
        }

        public static string CreateLogCatReport()
        {
            var sb = new Java.Lang.StringBuilder();
            sb.Append("===== logcat =====");

            try
            {
                var r = Java.Lang.Runtime.GetRuntime().Exec(new[] { "logcat", "-d", "Mono:I", "MARK5:V", "*:S" });
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
    }
}
