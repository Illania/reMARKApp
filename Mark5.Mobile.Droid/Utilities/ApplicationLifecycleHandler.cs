using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Support.V7.Preferences;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Utilities
{
    public class ApplicationLifecycleHandler : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        const string AuthenticationFragmentTag = "authenticationFragmentTag";
        const string lastClosedKey = "lastClosedKey";

        bool authenticated;
        int activitiesStarted;

        public bool ApplicationVisible => activitiesStarted > 0;

        public void OnActivityStarted(Activity activity)
        {
            if (activity is SplashActivity)
                return;

            if (!authenticated &&
                 !ApplicationVisible &&
                 activity is BaseAppCompatActivity bca &&
                 PlatformConfig.Preferences.AuthorizationEnabled &&
                 ShouldAuthenticate() &&
                 IsAuthenticationPossible(activity))
            {
                if ((AuthenticationDialogFragment)bca.SupportFragmentManager.FindFragmentByTag(AuthenticationFragmentTag) == null)
                {
                    var authFragment = new AuthenticationDialogFragment();
                    authFragment.Show(bca.SupportFragmentManager, AuthenticationFragmentTag);
                }
            }
            else
                ResetLastClosedTime();

            activitiesStarted++;
        }

        public void OnActivityStopped(Activity activity)
        {
            if (activity is SplashActivity)
                return;

            activitiesStarted--;
            authenticated = false;

            if (!ApplicationVisible && PlatformConfig.Preferences.AuthorizationEnabled)
                SaveLastClosedTime();
        }

        void SaveLastClosedTime()
        {
            var prefManager = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var editor = prefManager.Edit();
            editor.PutString(lastClosedKey, DateTime.UtcNow.ToString("s"));
            editor.Apply();
        }

        DateTime? GetLastClosedTime()
        {
            var prefManager = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var timeString = prefManager.GetString(lastClosedKey, string.Empty);
            if (timeString == string.Empty)
                return null;

            return DateTime.SpecifyKind(Convert.ToDateTime(timeString), DateTimeKind.Utc);
        }

        void ResetLastClosedTime()
        {
            var prefManager = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var editor = prefManager.Edit();
            editor.PutString(lastClosedKey, string.Empty);
            editor.Apply();
        }

        bool ShouldAuthenticate()
        {
            var lastTime = GetLastClosedTime();
            if (lastTime == null)
                return false;

            var timeDifference = DateTime.UtcNow - lastTime.Value;
            return timeDifference.TotalMinutes >= PlatformConfig.Preferences.AuthorizationInterval;
        }

        public void OnAuthenticationSuccessful()
        {
            authenticated = true;
            ResetLastClosedTime();
        }

        bool IsAuthenticationPossible(Activity activity)
        {
            var keyguardManager = (KeyguardManager)activity.GetSystemService(Context.KeyguardService);
            var fingerprintManager = FingerprintManagerCompat.From(activity);

            return keyguardManager.IsKeyguardSecure || (fingerprintManager.IsHardwareDetected && fingerprintManager.HasEnrolledFingerprints);
        }

        #region Unused callbacks

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            //Nothing to do
        }

        public void OnActivityDestroyed(Activity activity)
        {
            //Nothing to do
        }

        public void OnActivityResumed(Activity activity)
        {
            //Nothing to do
        }

        public void OnActivityPaused(Activity activity)
        {
            //Nothing to do
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            //Nothing to do
        }

        #endregion
    }
}