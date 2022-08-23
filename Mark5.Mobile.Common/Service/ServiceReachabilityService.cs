using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Polly;

namespace Mark5.Mobile.Common.Service
{
    public class ServiceReachabilityService : AbstractService, IServiceReachabilityService
    {

        public ServiceReachabilityService() : base(3 * 1000, false) { }
        private volatile bool _isReachable = false;

        protected override async Task Work(CancellationToken ct)
        {
            CommonConfig.Logger.Info("Starting service reachability check task...");
            var ci = Managers.ActiveConnectionInfo;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    if (ci != null)
                    {
                        var url = $"{(ci.SslMode == SslMode.Off ? "http" : "https")}://{ci.Hostname}:{ci.Port}";

                        static async Task<bool> RetryServiceConnect()
                        {
                            bool policyResult = await Policy
                                .HandleResult<bool>(r => r == false)
                                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2))
                                .ExecuteAsync(async () =>
                                {
                                    var result = await CommonConfig.Reachability.CheckWithServiceConnection();
                                    return result;
                                });

                            return policyResult;
                        }
                   
                        var isReachable = await RetryServiceConnect();

                       if(isReachable != _isReachable)
                            CommonConfig.Reachability.RefreshServiceReachability(isReachable);

                        _isReachable = isReachable;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Unexpected error in service reachability check task!", ex);
            }

            CommonConfig.Logger.Info("Stopped service reachability check task...");

        }
    }
}



