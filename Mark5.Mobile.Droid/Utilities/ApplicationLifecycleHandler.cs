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

        Stopwatch stopWatch;
        int activitiesStarted;
        Orientation currentOrientation;

        public ApplicationLifecycleHandler(Orientation initialOrientation)
        {
            stopWatch = new Stopwatch();
            currentOrientation = initialOrientation;
        }

        public void OnActivityStarted(Activity activity)
        {
            if (activity.Resources.Configuration.Orientation != currentOrientation && activitiesStarted > 0)
            {
                currentOrientation = activity.Resources.Configuration.Orientation;
                activitiesStarted++;
            }
            else
            {
                if (!ApplicationVisible)
                {
                    if (PlatformConfig.Preferences.AuthEnabled)
                    {
                        stopWatch.Stop();
                        
                        if (!(activity.GetType() == typeof(FingerprintActivity)) && stopWatch.Elapsed.Minutes >= PlatformConfig.Preferences.AuthInterval) //settings
                        {
                            KeyguardManager keyguardManager = (KeyguardManager)activity.GetSystemService(Context.KeyguardService);
                            FingerprintManagerCompat fingerprintManager = FingerprintManagerCompat.From(activity);
                            if (fingerprintManager.HasEnrolledFingerprints && keyguardManager.IsKeyguardSecure)
                            {
                                activity.StartActivity(FingerprintActivity.CreateIntent(activity));
                            } 
                            else if(keyguardManager.IsKeyguardSecure)
                            {
                                //ask for pin    
                            }
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
                    if (PlatformConfig.Preferences.AuthEnabled)
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