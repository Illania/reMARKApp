using System;
using reMark.Mobile.Common;

namespace reMark.Mobile.IOS.Service
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