using Android.App;
using Android.OS;
using System.Diagnostics;

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

        public ApplicationLifecycleHandler()
        {
            stopWatch = new Stopwatch();
        }

        public void OnActivityStarted(Activity activity)
        {
            if(stopWatch.ElapsedMilliseconds < 500)
                activitiesStarted++;
            else
            {
                if (!ApplicationVisible)
                {
                    if (PlatformConfig.Preferences.FingerPrintAuthEnabled)
                    {
                        stopWatch.Stop();
                        
                        if (!(activity.GetType() == typeof(FingerprintActivity)) && stopWatch.Elapsed.Minutes >= PlatformConfig.Preferences.FingerPrintAuthInterval) //settings
                        {
                            activity.StartActivity(FingerprintActivity.CreateIntent(activity));
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