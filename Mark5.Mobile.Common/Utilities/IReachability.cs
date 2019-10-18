using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public interface IReachability
    {
        bool IsReachable { get; set; }
        bool IsCheckingReachability { get; }
        event EventHandler RefreshingReachability;

        event EventHandler<ReachabilityRefreshedEventArgs> ReachabilityRefreshed;

        void OnPause();

        Task<bool> Refresh(ReachabilityMode mode = ReachabilityMode.NetworkAvailability | ReachabilityMode.Service, bool testOnly = false);

        Task<ConnectionDiagnosticModel> ConnectionDiagnostics();

        bool IsWifiConnected();

        bool IsMobileDataEnabled();

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