using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Polly;

namespace Mark5.Mobile.Common.Service
{
    public class ServiceReachabilityService : AbstractService, IServiceReachabilityService
    {
        public ServiceReachabilityService() : base(3 * 1000, false) { }
        private volatile bool _isReachable;

        protected override async Task Work(CancellationToken ct)
        {
            CommonConfig.Logger.Info("Starting service reachability check task...");
            var ci = Managers.ActiveConnectionInfo;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    if (ci == null) 
                        continue;

                    static async Task<bool> RetryServiceConnect()
                    {
                        var policyResult = await Policy
                            .HandleResult<bool>(r => r == false)
                            .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2))
                            .ExecuteAsync(async () =>
                            {
                                var result = await CommonConfig.Reachability.CheckWithServiceConnection();
                                return result;
                            });

                        return policyResult;
                    }
                        
                    var isReachable = await RetryServiceConnect();
                    if (isReachable == _isReachable) 
                        continue;
                    
                    CommonConfig.Reachability.RefreshServiceReachability(isReachable);
                    _isReachable = isReachable;
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