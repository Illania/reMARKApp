using Android.App;
using Android.Content;
using Android.Support.V7.Preferences;
using Android.Webkit;
using Mark5.Mobile.Common.Utilities;
namespace Mark5.Mobile.Droid.Utilities
{
    public class OnBoardingUtilities : Java.Lang.Object, IOnBoardingUtilities
    {
        const string appVersionKey = "latestAppVersionKey";

        readonly Context context;

        public OnBoardingUtilities(Context context)
        {
            this.context = context;
        }

        public void SaveAppVersionCode()
        {
            var pi = context.PackageManager.GetPackageInfo(context.PackageName, 0);

            var prefManager = PreferenceManager.GetDefaultSharedPreferences(context);
            var editor = prefManager.Edit();
            editor.PutInt(appVersionKey, pi.VersionCode);
        }

        bool ApplicationHasBeenUpdated()
        {
            var pi = context.PackageManager.GetPackageInfo(context.PackageName, 0);

            var storedVersion = PreferenceManager.GetDefaultSharedPreferences(context).GetInt(appVersionKey, 0);

            return storedVersion != 0 && pi.VersionCode > storedVersion;
        }

        public void TryShowingOnBoardingDialog()
        {
            //if(ApplicationHasBeenUpdated(context))
            //{
                SaveAppVersionCode();

                var webView = new WebView(context);
                
                AlertDialog.Builder builder = new AlertDialog.Builder(context)
                    .SetTitle(context.GetString(Resource.String.whatsnew))
                    .SetView(webView)
                    .SetPositiveButton(Resource.String.ok, (sender, e) =>
                {
                    var dialog = sender as AlertDialog;
                    dialog.Dismiss();
                });
                
                builder.Show();
            //}
        }
    }
}