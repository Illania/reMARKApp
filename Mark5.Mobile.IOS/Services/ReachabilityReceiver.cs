//
// Project: Mark5.Mobile.IOS
// File: ReachabilityReceiver.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

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
            CommonConfig.ReachabilityService.Refresh();
        }
    }
}