﻿using System;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Utilities
{
    public interface IReachability
    {
        bool IsReachable { get; }
        bool IsCheckingReachability { get; }
        event EventHandler RefreshingReachability;

        event EventHandler<ReachabilityRefreshedEventArgs> ReachabilityRefreshed;

        Task<bool> Refresh(ReachabilityMode mode = ReachabilityMode.NetworkAvailability | ReachabilityMode.Service, bool testOnly = false);
    }

    public class ReachabilityRefreshedEventArgs : EventArgs
    {
        public bool Changed { get; }
        public bool IsReachable { get; }

        public ReachabilityRefreshedEventArgs(bool changed, bool isReachable)
        {
            Changed = changed;
            IsReachable = isReachable;
        }
    }

    [Flags]
    public enum ReachabilityMode
    {
        NetworkAvailability = 1,
        Google = 2,
        ServiceConnection = 4,
        Service = 8
    }
}