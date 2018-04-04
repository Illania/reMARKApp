using System.Diagnostics;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Hardware.Fingerprint;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Utilities
{
    public class ApplicationLifecycleHandler : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        const string AuthenticationFragmentTag = "authenticationFragmentTag";

        bool authenticated;
        int activitiesStarted;
        Stopwatch stopWatch;

        public bool ApplicationVisible => activitiesStarted > 0;

        public ApplicationLifecycleHandler()
        {
            stopWatch = new Stopwatch();
        }

        public void OnActivityStarted(Activity activity)
        {
            if (!authenticated &&
                 !ApplicationVisible &&
                 activity is BaseAppCompatActivity &&
                 PlatformConfig.Preferences.AuthorizationEnabled &&
                 stopWatch.Elapsed.Minutes >= PlatformConfig.Preferences.AuthorizationInterval &&
                 IsAuthenticationPossible(activity))
            {
                if ((AuthenticationDialogFragment)activity.FragmentManager.FindFragmentByTag(AuthenticationFragmentTag) == null)
                {
                    var authFragment = new AuthenticationDialogFragment();
                    authFragment.Show(activity.FragmentManager, AuthenticationFragmentTag);
                }
            }
            else
            {
                stopWatch.Reset();
            }

            activitiesStarted++;
        }

        public void OnActivityStopped(Activity activity)
        {
            activitiesStarted--;
            authenticated = false;

            if (!ApplicationVisible && PlatformConfig.Preferences.AuthorizationEnabled)
                stopWatch.Start();
        }

        public void OnAuthenticationSuccessful()
        {
            authenticated = true;
            stopWatch.Reset();
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