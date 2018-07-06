using Android.App;
using Android.Content;
using Android.Support.V7.Preferences;
using Android.Webkit;
using Mark5.Mobile.Common;
using System.IO;

namespace Mark5.Mobile.Droid.Utilities
{
    public class OnBoardingUtilities
    {
        const string appVersionKey = "latestAppVersionKey";
        readonly Context context;
        readonly int currentVersionCode;

        public OnBoardingUtilities(Context context)
        {
            this.context = context;
            currentVersionCode = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionCode;
        }

        public void SaveAppVersionCode()
        {
            var prefManager = PreferenceManager.GetDefaultSharedPreferences(context);
            var editor = prefManager.Edit();
            editor.PutInt(appVersionKey, currentVersionCode);
            editor.Commit();
        }

        public void TryShowingOnBoardingDialog()
        {
            if (ApplicationHasBeenUpdated())
            {
                SaveAppVersionCode();

                var webView = new WebView(context);
                string changeloghtml = "";

                try
                {
                    //TODO: Add proper chagelog.
                    using (var sr = new StreamReader(context.Assets.Open("changelogs/changelog_" + currentVersionCode + ".html")))
                        changeloghtml = sr.ReadToEnd();
                }
                catch (Java.IO.FileNotFoundException ex)
                {
                    CommonConfig.Logger.Error("There is no changelog for this version code!", ex);
                    return;
                }

                webView.StopLoading();
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

        bool ApplicationHasBeenUpdated()
        {
            var storedVersionCode = PreferenceManager.GetDefaultSharedPreferences(context).GetInt(appVersionKey, 0);

            return currentVersionCode > storedVersionCode;
        }
    }
}