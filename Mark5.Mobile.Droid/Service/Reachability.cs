using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Testers;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Service
{
    public class Reachability : IReachability
    {
        const string GoogleRequestUrl = "http://clients3.google.com/generate_204";

        public bool IsReachable { get; private set; }

        public bool IsCheckingReachability { get; private set; }

        public event EventHandler RefreshingReachability = delegate { };

        public event EventHandler<ReachabilityRefreshedEventArgs> ReachabilityRefreshed = delegate { };

        CancellationTokenSource cancellationTokenSource;

        private static Reachability instance;

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
            if (result && mode.HasFlag(ReachabilityMode.ServiceConnection))
                result &= await CheckWithServiceConnection();
            if (result && mode.HasFlag(ReachabilityMode.Service))
                result &= await CheckWithService();

            IsCheckingReachability = false;

            if (!testOnly)
            {
                var lastResult = IsReachable;
                IsReachable = result;

                CommonConfig.Logger.Info($"Reachability checked: {result}");

                ReachabilityRefreshed(this, new ReachabilityRefreshedEventArgs(lastResult != result, result));
            }

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }

            if (!result)
            {
                cancellationTokenSource = new CancellationTokenSource();
                CheckServiceAvailabilityContinuously(cancellationTokenSource.Token);
            }

            return result;
        }

        public bool CheckNetworkAvailability()
        {
            var cm = (ConnectivityManager) Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
            var result = cm.ActiveNetworkInfo?.IsConnected ?? false;

            CommonConfig.Logger.Info($"Network availability: {result}");

            return result;
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

        public async Task<bool> CheckWithServiceConnection()
        {
            try
            {
                var ci = Managers.ActiveConnectionInfo;
                var url = $"{(ci.SslMode == SslMode.Off ? "http" : "https")}://{ci.Hostname}:{ci.Port}/app3";

                using (var httpClient = new HttpClient(CommonConfig.HttpClientHandler())
                {
                    Timeout = new TimeSpan(0, 0, 2)
                })
                using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    var result = response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest;

                    CommonConfig.Logger.Info($"Service connection availability: {result}. [status={response.StatusCode}]");

                    return result;
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Info("Cannot check service connection availability", ex);

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

                CommonConfig.Logger.Info($"Service availability: {result}");

                return result;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Info("Cannot check service availability", ex);

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
                    ReachabilityRefreshed(this, new ReachabilityRefreshedEventArgs(true, true));
                }

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }
}