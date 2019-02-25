using Android.Content;
using Android.Net;
using Android.OS;
using Mark5.Mobile.Common;
using static Android.Net.ConnectivityManager;

namespace Mark5.Mobile.Droid.Service
{
    public class ReachabilityMonitor : NetworkCallback
    {
        readonly NetworkRequest networkRequest;
        bool registered;

        public ReachabilityMonitor()
        {
            networkRequest = new NetworkRequest.Builder().AddTransportType(TransportType.Cellular).AddTransportType(TransportType.Wifi).Build();
        }

        public void Register(Context context)
        {
            if (registered)
                return;

            registered = true;

            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            cm.RegisterNetworkCallback(networkRequest, this);
        }

        public void Unregister(Context context)
        {
           if (!registered)
                return;

            registered = false;

            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            cm.UnregisterNetworkCallback(this);
        }

        public override void OnAvailable(Network network)
        {
            base.OnAvailable(network);

            CommonConfig.Logger.Info("Connectivity changed");
            new Handler(Looper.MainLooper).Post(() => CommonConfig.Reachability.Refresh());
        }

        public override void OnCapabilitiesChanged(Network network, NetworkCapabilities networkCapabilities)
        {
            base.OnCapabilitiesChanged(network, networkCapabilities);
        }

        public override void OnLost(Network network)
        {
            base.OnLost(network);

            CommonConfig.Logger.Info("Connectivity changed");
            new Handler(Looper.MainLooper).Post(() => CommonConfig.Reachability.Refresh());
        }
    }
}