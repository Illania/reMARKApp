using System;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.IOS.Services
{
    public class ReachabilityReceiver
    {
        public void Register()
        {
            ReachabilityProvider.InternetConnectionStatus();
            ReachabilityProvider.ReachabilityChanged += Reachability_ReachabilityChanged;
        }

        public void Unregister()
        {
            ReachabilityProvider.ReachabilityChanged -= Reachability_ReachabilityChanged;
        }

        void Reachability_ReachabilityChanged(object sender, EventArgs e)
        {
            CommonConfig.Reachability.Refresh();
        }
    }
}