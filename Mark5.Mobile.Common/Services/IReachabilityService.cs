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

        Task<bool> IsServiceReachable();

        event EventHandler<ReachabilityChangedEventArgs> ReachabilityChanged;
    }

    public class ReachabilityChangedEventArgs : EventArgs
    {

        public bool IsReachable
        {
            get;
            private set;
        }

        public bool WasReachable
        {
            get;
            private set;
        }

        public ReachabilityChangedEventArgs(bool isReachable, bool wasReachable)
        {
            IsReachable = isReachable;
            WasReachable = wasReachable;
        }
    }
}


