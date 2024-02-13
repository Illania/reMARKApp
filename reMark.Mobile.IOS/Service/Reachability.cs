using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using reMark.Mobile.Classes;
using reMark.Mobile.Classes.Enum;
using reMark.Mobile.Classes.Model;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Testers;
using reMark.Mobile.Common.Utilities;

namespace reMark.Mobile.IOS.Service
{
    public class Reachability : IReachability
    {
        const string GoogleRequestUrl = "http://clients3.google.com/generate_204";

        public bool IsReachable { get; private set; }

        public bool IsCheckingReachability { get; private set; }

        public event EventHandler RefreshingReachability = delegate { };

        public event EventHandler<ReachabilityRefreshedEventArgs> ReachabilityRefreshed = delegate { };

        private static Reachability instance;

        CancellationTokenSource cancellationTokenSource;

        private Reachability()
        {
            IsReachable = CheckNetworkAvailability();
        }

        public static Reachability Instance
        {
            get
            {
                if (instance == null)
                    instance = new Reachability();

                return instance;
            }
        }

        public async Task<bool> Refresh(ReachabilityMode mode = ReachabilityMode.NetworkAvailability | ReachabilityMode.Service, bool testOnly = false)
        {
            IsCheckingReachability = true;

            if (!testOnly)
                RefreshingReachability(this, EventArgs.Empty);

            var result = true;

            if (result && mode.HasFlag(ReachabilityMode.NetworkAvailability))
                result &= CheckNetworkAvailability();
            if (result && mode.HasFlag(ReachabilityMode.Google))
                result &= await CheckWithGoogle();
            if (result && mode.HasFlag(ReachabilityMode.Service))
                result &= await CheckWithService();

            IsCheckingReachability = false;

            if (!testOnly)
            {
                var lastResult = IsReachable;
                IsReachable = result;

                CommonConfig.Logger.Info($"Reachability checked: {result}");

                ReachabilityRefreshed(this, new ReachabilityRefreshedEventArgs(lastResult != result, result));

                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource = null;
                }

                if (!result)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    _ = CheckServiceAvailabilityContinuously(cancellationTokenSource.Token);
                }
            }

            return result;
        }

        public bool CheckNetworkAvailability()
        {
            return ReachabilityProvider.InternetConnectionStatus() != NetworkStatus.NotReachable;
        }

        public async Task<bool> CheckWithGoogle()
        {
            try
            {
                using (var httpClient = new HttpClient(CommonConfig.HttpClientHandler())
                {
                    Timeout = new TimeSpan(0, 0, 2)
                })
                using (var response = await httpClient.GetAsync(GoogleRequestUrl))
                {
                    var result = response.StatusCode == HttpStatusCode.NoContent && (await response.Content.ReadAsByteArrayAsync()).Length == 0;

                    CommonConfig.Logger.Info($"Google availability: {result}");

                    return result;
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Info("Cannot check Google availability", ex);

                return false;
            }
        }

        public async Task<bool> CheckWithService()
        {
            try
            {
                var tester = ConnectionTesterFactory.Create();
                if (!await tester.CanTest())
                {
                    CommonConfig.Logger.Info("Cannot test service availability");

                    return false;
                }

                var result = await tester.Test();

                if(result == false)
                    CommonConfig.Logger.Info($"Service availability: {result}");

                return result;
            }
            catch (Exception)
            {
                CommonConfig.Logger.Info($"Service availability: false");

                return false;
            }
        }

        async Task CheckServiceAvailabilityContinuously(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await CheckWithService();

                if (response)
                {
                    cancellationTokenSource.Cancel();
                    IsReachable = response;
                    ReachabilityRefreshed(this, new ReachabilityRefreshedEventArgs(true, true));
                }

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        public void OnPause()
        {
        }

        public async Task<ConnectionDiagnosticModel> ConnectionDiagnostics()
        {
            try
            {
                var tester = ConnectionTesterFactory.Create();
                if (!await tester.CanTest())
                {
                    CommonConfig.Logger.Info("Configuration file is missing connection info");
                    return new ConnectionDiagnosticModel(ConnectionDiagnosticModel.ErrorCode.NoConfigurationInfo);
                }

                ConnectionDiagnosticModel result = await tester.ConnectionDiagnostics();
                CommonConfig.Logger.Info($"Service availability: {result}");
                return result;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Info("Cannot check service availability", ex);
                return new ConnectionDiagnosticModel(ConnectionDiagnosticModel.ErrorCode.UncaughtException);
            }
        }

        public bool IsWifiConnected()
        {
            NetworkStatus networkStatus = ReachabilityProvider.InternetConnectionStatus();
            return networkStatus == NetworkStatus.ReachableViaWiFiNetwork;
        }

        public bool IsMobileDataEnabled()
        {
            NetworkStatus networkStatus = ReachabilityProvider.InternetConnectionStatus();
            return networkStatus == NetworkStatus.ReachableViaCarrierDataNetwork;
        }

        public void RefreshServiceReachability(bool isReachable)
        {
            IsReachable = isReachable;
            ReachabilityRefreshed(this, new ReachabilityRefreshedEventArgs(true, isReachable));
        }


        public SourceType GetReachabilitySourceType() => IsReachable ? SourceType.Remote : SourceType.Local; 

    }
}