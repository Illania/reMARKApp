//
// Project: Mark5.Mobile.IOS
// File: ReachabilityService.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Services;
using Mark5.Mobile.Common.Tester;

namespace Mark5.Mobile.IOS.Services
{
    
    public class ReachabilityService : IReachabilityService
    {
        
        const string GoogleRequestUrl = "http://clients3.google.com/generate_204";

        public bool IsReachable
        {
            get;
            private set;
        }

        public event EventHandler RefreshingReachability = delegate { };

        public event EventHandler<ReachabilityRefreshedEventArgs> ReachabilityRefreshed = delegate { };

        public ReachabilityService()
        {
            IsReachable = CheckNetworkAvailability();
        }

        public async Task<bool> Refresh(ReachabilityMode mode = ReachabilityMode.ServiceConnection, bool testOnly = false)
        {
            if (!testOnly)
            {
                RefreshingReachability(this, EventArgs.Empty);
            }

            var result = true;

            if (result && mode.HasFlag(ReachabilityMode.NetworkAvailability))
            {
                result &= CheckNetworkAvailability();
            }
            if (result && mode.HasFlag(ReachabilityMode.Google))
            {
                result &= await CheckWithGoogle();
            }
            if (result && mode.HasFlag(ReachabilityMode.ServiceConnection))
            {
                result &= await CheckWithServiceConnection();
            }
            if (result && mode.HasFlag(ReachabilityMode.Service))
            {
                result &= await CheckWithService();
            }

            if (!testOnly)
            {
                var lastResult = IsReachable;
                IsReachable = result;

                CommonConfig.Logger.Info($"Reachability checked: {result}");

                ReachabilityRefreshed(this, new ReachabilityRefreshedEventArgs(lastResult != result, result));
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
                    var result = response.StatusCode == HttpStatusCode.OK;

                    CommonConfig.Logger.Info($"Service connection availability: {result}");

                    return result;
                }
            }
            catch (Exception)
            {
                CommonConfig.Logger.Info($"Service connection availability: false");

                return false;
            }
        }

        public async Task<bool> CheckWithService()
        {
            try
            {
                var tester = TesterFactory.Create();
                if (!await tester.CanTest())
                {
                    CommonConfig.Logger.Info("Cannot test service availability");

                    return false;
                }

                var result = await tester.Test();

                CommonConfig.Logger.Info($"Service availability: {result}");

                return result;
            }
            catch (Exception)
            {
                CommonConfig.Logger.Info($"Service availability: false");

                return false;
            }
        }
    }
}
