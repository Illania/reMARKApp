using System;
using System.Threading.Tasks;
using reMark.Mobile.Classes.Enum;
using reMark.Mobile.Classes.Model;

namespace reMark.Mobile.Classes
{
    public interface IReachability
    {
        bool IsReachable { get; }
        bool IsCheckingReachability { get; }
        event EventHandler RefreshingReachability;

        event EventHandler<ReachabilityRefreshedEventArgs> ReachabilityRefreshed;

        void OnPause();

        Task<bool> Refresh(ReachabilityMode mode = 
            ReachabilityMode.NetworkAvailability | ReachabilityMode.Service, bool testOnly = false);

        Task<ConnectionDiagnosticModel> ConnectionDiagnostics();

        bool IsWifiConnected();

        bool IsMobileDataEnabled();

        SourceType GetReachabilitySourceType();

        void RefreshServiceReachability(bool isReachable);

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
        Service = 4
    }
}