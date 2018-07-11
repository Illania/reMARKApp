using Android.App;
using Android.Content;
using Android.Support.V7.Preferences;
using Android.Webkit;
using System.IO;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class OnBoardingUtilities
    {
        const string appVersionKey = "latestAppVersionKey";

        public static void SaveAppVersionCode(Context context)
        {
            var currentVersionCode = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionCode;
            var prefManager = PreferenceManager.GetDefaultSharedPreferences(context);
            var editor = prefManager.Edit();
            editor.PutInt(appVersionKey, currentVersionCode);
            editor.Commit();
        }

        public static void ShowOnBoardingIfNecessary(Context context)
        {
            if (ApplicationHasBeenUpdated(context))
            {
                SaveAppVersionCode(context);

                var currentVersionCode = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionCode;
                var webView = new WebView(context);
                string changeloghtml = "";

                //TODO: Add proper chagelog.
                using (var sr = new StreamReader(context.Assets.Open("changelogs/changelog_" + currentVersionCode + ".html")))
                    changeloghtml = sr.ReadToEnd();

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

        static bool ApplicationHasBeenUpdated(Context context)
        {
            var currentVersionCode = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionCode;
            var storedVersionCode = PreferenceManager.GetDefaultSharedPreferences(context).GetInt(appVersionKey, 0);

            return currentVersionCode > storedVersionCode;
        }
    }
}