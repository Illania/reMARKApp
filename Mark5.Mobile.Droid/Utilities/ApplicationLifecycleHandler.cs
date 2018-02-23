using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Diagnostics;

namespace Mark5.Mobile.Droid.Utilities
{
    public class ApplicationLifecycleHandler : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        public bool ApplicationVisible
        {
            get 
            { 
                return acitivitiesStarted > 0; 
            }
        }

        Stopwatch stopWatch;
        int acitivitiesStarted;
        
        public ApplicationLifecycleHandler()
        {
            stopWatch = new Stopwatch();
        }

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

        public void OnActivityStarted(Activity activity)
        {
            if(!ApplicationVisible)
            {
                stopWatch.Stop();

                if(!activity.GetType().IsAssignableFrom(typeof(FingerprintActivity)) && stopWatch.Elapsed.Minutes >= PlatformConfig.Preferences.FingerPrintAuthInterval) //settings
                {
                    activity.StartActivity(FingerprintActivity.CreateIntent(activity));
                }
                stopWatch.Reset();
            }
            acitivitiesStarted++;
        }

        public void OnActivityStopped(Activity activity)
        {
            acitivitiesStarted--;
            if(!ApplicationVisible)
            {
                stopWatch.Start();
            }
        }
    }
}
