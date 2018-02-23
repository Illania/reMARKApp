using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Diagnostics;

namespace Mark5.Mobile.Droid.Utilities
{
    public class ApplicationLifecycleHandler : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        public bool IsApplicationVisible
        {
            get 
            { 
                return acitivitiesStarted > 0; 
            }
        }

        public ApplicationLifecycleHandler()
        {
            stopWatch = new Stopwatch();
        }

        public bool ShouldBeShown;
        Stopwatch stopWatch;
        int acitivitiesStarted;

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
            if(!IsApplicationVisible)
            {
                stopWatch.Stop();

                if(stopWatch.Elapsed.Seconds > 10 && ShouldBeShown) //settings
                {
                    Toast.MakeText(activity, Resource.String.fingerprint_success, ToastLength.Long).Show();
                    activity.StartActivity(FingerprintActivity.CreateIntent(activity));
                }
                stopWatch.Reset();

            }
            acitivitiesStarted++;
        }

        public void OnActivityStopped(Activity activity)
        {
            acitivitiesStarted--;
            if(!IsApplicationVisible)
            {
                Toast.MakeText(activity, Resource.String.fingerprint_try_again, ToastLength.Long).Show();
                stopWatch.Start();
                ShouldBeShown = true;
            }
        }
    }
}
