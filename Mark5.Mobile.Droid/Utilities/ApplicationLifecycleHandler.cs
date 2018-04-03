using Android.App;
using Android.OS;
using System.Diagnostics;
using Android.Content.Res;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Content;

namespace Mark5.Mobile.Droid.Utilities
{
    public class ApplicationLifecycleHandler : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        const string AuthenticationFragmentTag = "authenticationFragmentTag";

        int activitiesStarted;
        Orientation currentOrientation;
        Stopwatch stopWatch;

        public bool ApplicationVisible => activitiesStarted > 0;

        public ApplicationLifecycleHandler(Orientation initialOrientation)
        {
            stopWatch = new Stopwatch();
            currentOrientation = initialOrientation;
        }

        public void OnActivityStarted(Activity activity)
        {
            if (!(activity is BaseAppCompatActivity))
                return;

            var authFragment = (AuthenticationDialogFragment)activity.FragmentManager.FindFragmentByTag(AuthenticationFragmentTag);
            if (authFragment == null)
            {
                authFragment = new AuthenticationDialogFragment();
                authFragment.Show(activity.FragmentManager, AuthenticationFragmentTag);
            }
            else
            {
                authFragment.DismissIfAuthenticated();
            }

            //if (activity.Resources.Configuration.Orientation != currentOrientation)
            //{
            //    currentOrientation = activity.Resources.Configuration.Orientation;
            //    activitiesStarted++;
            //}
            //else
            //{
            //    if (!ApplicationVisible)
            //    {
            //        if (PlatformConfig.Preferences.AuthorizationEnabled)
            //        {
            //            stopWatch.Stop();

            //            if (stopWatch.Elapsed.Minutes >= PlatformConfig.Preferences.AuthorizationInterval)
            //            {
            //                var keyguardManager = (KeyguardManager)activity.GetSystemService(Context.KeyguardService);
            //                var fingerprintManager = FingerprintManagerCompat.From(activity);
            //                if (fingerprintManager.HasEnrolledFingerprints && keyguardManager.IsKeyguardSecure)
            //                {
            //                    var f = new Ui.Fragments.FingerprintDialogFragment();
            //                    f.Show(activity.FragmentManager, "test_tag");
            //                }
            //            }

            //            stopWatch.Reset();
            //        }
            //    }
            //    activitiesStarted++;
            //}
        }

        public void OnActivityStopped(Activity activity)
        {
            activitiesStarted--;
            if (!ApplicationVisible)
            {
                if (PlatformConfig.Preferences.AuthorizationEnabled)
                    stopWatch.Start();
            }
        }

        bool IsAuthorizationPossible(Activity activity)
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