//
// Project: Mark5.Mobile.Common
// File: IReachabilityService.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Services
{

    public interface IReachabilityService
    {

        bool IsReachable { get; }

        event EventHandler RefreshingReachability;

        event EventHandler<ReachabilityRefreshedEventArgs> ReachabilityRefreshed;

        Task<bool> Refresh(ReachabilityMode mode = ReachabilityMode.NetworkAvailability | ReachabilityMode.ServiceConnection, bool testOnly = false);
    }

    public class ReachabilityRefreshedEventArgs : EventArgs
    {

        public bool Changed
        {
            get;
            private set;
        }

        public bool IsReachable
        {
            get;
            private set;
        }

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

