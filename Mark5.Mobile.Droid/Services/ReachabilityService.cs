//
// Project: Mark5.Mobile.Droid
// File: ReachabilityService.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Mark5.Mobile.Common.Services;
using Mark5.Mobile.Common.Tester;

namespace Mark5.Mobile.Droid.Services
{

    public class ReachabilityService : IReachabilityService
    {

        const string GoogleRequestUrl = "http://clients3.google.com/generate_204";

        public bool IsReachable
        {
            get
            {
                return lastResult;
            }
        }

        public event EventHandler RefreshingReachability = delegate { };

        public event EventHandler<ReachabilityRefershedEventArgs> ReachabilityRefreshed = delegate { };

        bool lastResult;

        public ReachabilityService()
        {
            lastResult = CheckNetworkAvailability();
        }

        public async Task<bool> Refresh(ReachabilityMode mode = ReachabilityMode.NetworkAvailability | ReachabilityMode.Service, CancellationToken ct = default(CancellationToken))
        {
            RefreshingReachability(this, EventArgs.Empty);

            var result = true;

            if (result && mode.HasFlag(ReachabilityMode.NetworkAvailability))
            {
                result &= CheckNetworkAvailability();
            }
            if (result && mode.HasFlag(ReachabilityMode.Google))
            {
                result &= await CheckWithGoogle(ct);
            }
            if (result && mode.HasFlag(ReachabilityMode.Service))
            {
                result &= await CheckWithService(ct);
            }

            ReachabilityRefreshed(this, new ReachabilityRefershedEventArgs(lastResult != result, result));

            lastResult = result;

            return result;
        }

        public bool CheckNetworkAvailability()
        {
            var cm = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            return cm.ActiveNetworkInfo?.IsConnected ?? false;
        }

        public async Task<bool> CheckWithGoogle(CancellationToken ct = default(CancellationToken))
        {
            try
            {
                using (var httpClient = new HttpClient
                {
                    Timeout = new TimeSpan(0, 0, 2)
                })
                using (var response = await httpClient.GetAsync(GoogleRequestUrl, ct))
                {
                    return response.StatusCode != HttpStatusCode.NoContent && (await response.Content.ReadAsByteArrayAsync()).Length == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CheckWithService(CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var tester = TesterFactory.Create();
                if (!await tester.CanTest(ct))
                {
                    return false;
                }

                return await tester.Test(ct);
            }
            catch
            {
                return false;
            }
        }
    }
}

