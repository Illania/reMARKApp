using Android.App;
using Android.OS;
using System.Diagnostics;
using Android.Content.Res;
using Android.Content;
using Android.Support.V4.Hardware.Fingerprint;

namespace Mark5.Mobile.Droid.Utilities
{
    public class ApplicationLifecycleHandler : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        public bool ApplicationVisible
        {
            get
            {
                return activitiesStarted > 0;
            }
        }

        /* Used to check if the user has been redirected to the PIN Code activity. 
         * If it is the case, then when the user is returned to the app from the pin code activity, then the fingerprintactivity should not be started.
         */
        public bool RedirectedToPincodeActivity 
        {
            get; set;
        }

        int activitiesStarted;
        Orientation currentOrientation;
        Stopwatch stopWatch;

        public ApplicationLifecycleHandler(Orientation initialOrientation)
        {
            stopWatch = new Stopwatch();
            currentOrientation = initialOrientation;
        }

        public void OnActivityStarted(Activity activity)
        {
            if (activity.Resources.Configuration.Orientation != currentOrientation)
            {
                currentOrientation = activity.Resources.Configuration.Orientation;
                activitiesStarted++;
            }
            else
            {
                if (!ApplicationVisible)
                {
                    if (PlatformConfig.Preferences.FingerPrintAuthEnabled)
                    {
                        stopWatch.Stop();

                        if (!RedirectedToPincodeActivity && !(activity.GetType() == typeof(FingerprintActivity)) && stopWatch.Elapsed.Minutes >= PlatformConfig.Preferences.FingerPrintAuthInterval) 
                        {
                            var keyguardManager = (KeyguardManager)activity.GetSystemService(Context.KeyguardService);
                            var fingerprintManager = FingerprintManagerCompat.From(activity);
                            if (fingerprintManager.HasEnrolledFingerprints && keyguardManager.IsKeyguardSecure)
                            {
                                activity.StartActivity(FingerprintActivity.CreateIntent(activity));
                            } 
                        }
                        else 
                        {
                            RedirectedToPincodeActivity = false;
                        }
                        stopWatch.Reset();
                    }
                }
                activitiesStarted++;
            }
        }

        public void OnActivityStopped(Activity activity)
        {
            activitiesStarted--;
            if (!ApplicationVisible)
            {
                if (PlatformConfig.Preferences.FingerPrintAuthEnabled)
                    stopWatch.Start();
            }
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