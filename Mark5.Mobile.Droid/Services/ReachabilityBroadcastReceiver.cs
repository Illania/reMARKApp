using Android.App;
using Android.Content;
using Android.Net;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;

namespace Mark5.Mobile.Droid.Services
{
    public class ReachabilityBroadcastReceiver : BroadcastReceiver
    {
        bool registered;

        public void Register()
        {
            if (registered)
                return;

            registered = true;

            var intentFilter = new IntentFilter();
            intentFilter.AddAction(ConnectivityManager.ConnectivityAction);
            Application.Context.RegisterReceiver(this, intentFilter);
        }

        public void Unregister()
        {
            if (!registered)
                return;

            registered = false;

            Application.Context.UnregisterReceiver(this);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != ConnectivityManager.ConnectivityAction)
                return;

            CommonConfig.Logger.Info("Connectivity changed");

            GoAsync();
            CommonConfig.Reachability.Refresh().FireAndForget();
        }
    }
}