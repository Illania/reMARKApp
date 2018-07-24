using Android.App;
using Android.Content;
using Android.Support.V7.Preferences;
using Android.Webkit;
using Mark5.Mobile.Common;
using System;
using System.IO;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class OnBoardingUtilities
    {
        const string appVersionKey = "latestAppVersionKey";

        public static void ShowOnBoardingIfNecessary(Context context)
        {
            try
            {
                if (ApplicationHasBeenUpdated(context))
                {
                    SaveAppVersionName(context);

                    var currentVersionName = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
                    var changelogName = "changelog_" + currentVersionName + ".html";
                    var changelogPath = "changelogs/" + changelogName;
                    string changeloghtml;

                    if (Array.IndexOf(context.Assets.List("changelogs"), changelogName) < 0)
                        return;

                    using (var sr = new StreamReader(context.Assets.Open(changelogPath)))
                        changeloghtml = sr.ReadToEnd();

                    var webView = new WebView(context);
                    webView.LoadDataWithBaseURL(null, changeloghtml, "text/html", "UTF-8", null);

                    AlertDialog.Builder builder = new AlertDialog.Builder(context)
                        .SetTitle(context.GetString(Resource.String.whatsnew))
                        .SetView(webView)
                        .SetPositiveButton(Resource.String.ok, (sender, e) =>
                    {
                        var dialog = sender as AlertDialog;
                        dialog.Dismiss();
                    });

                    builder.Show();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error trying to show onboarding.", ex);
                return;
            }
        }

        static bool ApplicationHasBeenUpdated(Context context)
        {
            var currentVersionCode = float.Parse(context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName);
            var storedVersionCode = PreferenceManager.GetDefaultSharedPreferences(context).GetFloat(appVersionKey, 0);

            return currentVersionCode > storedVersionCode;
        }

        static void SaveAppVersionName(Context context)
        {
            var currentVersionCode = float.Parse(context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName);
            var prefManager = PreferenceManager.GetDefaultSharedPreferences(context);
            var editor = prefManager.Edit();
            editor.PutFloat(appVersionKey, currentVersionCode);
            editor.Commit();
        }
    }
}