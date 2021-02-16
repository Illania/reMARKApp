using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.Net;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Storage.AppFileStorage.Interface;
using Mark5.Mobile.Common.Synchronizer;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class Integration
    {
        public static void DialNumber(Context context, string formattedNumber)
        {
            var intent = new Intent(Intent.ActionDial);
            intent.SetData(Android.Net.Uri.Parse("tel:" + formattedNumber));
            context.StartActivity(intent);
        }

        public static void TextNumber(Context context, string formattedNumber)
        {
            var intent = new Intent(Intent.ActionView);
            intent.SetData(Android.Net.Uri.Parse("sms:" + formattedNumber));
            context.StartActivity(intent);
        }

        public static void OpenMap(Context context, string formattedAddress)
        {
            var intent = new Intent(Intent.ActionView);
            var uriString = "geo:0,0?q=" + System.Net.WebUtility.HtmlEncode(formattedAddress);
            intent.SetData(Android.Net.Uri.Parse(uriString));
            var chooserIntent = Intent.CreateChooser(intent, context.Resources.GetString(Resource.String.choose_application));
            context.StartActivity(chooserIntent);
        }

        public static bool IsConnectedToMeteredConnection()
        {
            var ctx = Application.Context;
            var cm = (ConnectivityManager)ctx.GetSystemService(Context.ConnectivityService);
            return ConnectivityManagerCompat.IsActiveNetworkMetered(cm);
        }

        public static void CopyToClipboard(Context context, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var cm = (ClipboardManager)context.GetSystemService(Context.ClipboardService);
            cm.PrimaryClip = ClipData.NewPlainText(text, text);

            Toast.MakeText(context, Resource.String.copied_to_clipboard, ToastLength.Short).Show();
        }

        public static bool IsRootedMethod1()
        {
            var buildTags = Build.Tags;
            return buildTags != null && buildTags.Contains("test-keys");
        }

        public static bool IsRootedMethod2()
        {
            var paths = new[]
            {
                "/system/app/Superuser.apk",
                "/sbin/su",
                "/system/bin/su",
                "/system/xbin/su",
                "/data/local/xbin/su",
                "/data/local/bin/su",
                "/system/sd/xbin/su",
                "/system/bin/failsafe/su",
                "/data/local/su",
                "/su/bin/su"
            };
            foreach (var path in paths)
                if (new Java.IO.File(path).Exists())
                    return true;

            return false;
        }

        public static bool IsRootedMethod3()
        {
            Java.Lang.Process process = null;
            try
            {
                process = Java.Lang.Runtime.GetRuntime()
                    .Exec(new[]
                    {
                        "/system/xbin/which",
                        "su"
                    });
                var br = new Java.IO.BufferedReader(new Java.IO.InputStreamReader(process.InputStream));
                if (br.ReadLine() != null)
                    return true;

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

        public static async Task ClearData(Context context)
        {
            await Synchronizers.LocalRemindersSynchronizer.CancelAllReminders();

            var preferences = PreferenceManager.GetDefaultSharedPreferences(context);
            var editor = preferences.Edit();
            editor.Clear();
            editor.Commit();

            var foldersToRemove = new List<IFolder> { CommonConfig.DataFolder,
                CommonConfig.DatabaseFolder,
                CommonConfig.AttachmentsFolder,
                CommonConfig.DocumentsToUploadFolder,
                CommonConfig.DocumentWorkingCopyFolder };

            foreach (var folder in foldersToRemove)
            {
                try
                {
                    await folder.DeleteAsync();
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Info($"Unable to delete folder, path:{folder.Path}: {ex.Message}");
                }
            }
               
        }
    }
}