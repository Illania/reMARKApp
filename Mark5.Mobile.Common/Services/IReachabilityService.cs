//
// Project: Mark5.Mobile.Common
// File: IReachabilityService.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Services
{

    public interface IReachabilityService
    {

        event EventHandler RefreshingReachability;

        event EventHandler<ReachabilityRefershedEventArgs> ReachabilityRefreshed;

        Task<bool> Refresh(ReachabilityMode mode = ReachabilityMode.NetworkAvailability | ReachabilityMode.Service, CancellationToken ct = default(CancellationToken));
    }

    public class ReachabilityRefershedEventArgs : EventArgs
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

        public ReachabilityRefershedEventArgs(bool changed, bool isReachable)
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
        Service = 4
    }
}

