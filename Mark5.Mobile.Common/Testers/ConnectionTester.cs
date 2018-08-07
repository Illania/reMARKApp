using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Storage;
using Mark5.ServiceReference;
using DataContract = Mark5.ServiceReference.DataContract;
using Polly;

namespace Mark5.Mobile.Common.Testers
{
    public class ConnectionTester : IConnectionTester
    {
        public async Task<bool> CanTest(CancellationToken ct = default(CancellationToken))
        {
            return await FileSystemStorage.GetConnectionInfoAsync(ct) != null;
        }

        public async Task<bool> Test(CancellationToken ct = default(CancellationToken))
        {
            int retries = 0;
            const int attempts = 3;
            try
            {
                if (!await CanTest(ct))
                    return false;

                var ci = await FileSystemStorage.GetConnectionInfoAsync(ct);

                var proxy = AppServiceProxyFactory.Create(ci.SslMode != SslMode.Off,
                                                          ci.Hostname,
                                                          ci.Port,
                                                          CommonConfig.HttpClientHandler,
                                                          null,
                                                          null);

                var policy = Policy.Handle<Exception>().WaitAndRetryAsync(attempts, attempt => TimeSpan.FromMilliseconds(200), (exception, calculatedWaitDuration) =>
                {
                    retries++;
                    CommonConfig.Logger.Info($"Failed to Ping server after {retries} retry");
                });
                await policy.ExecuteAsync(async () => 
                { 
                    await proxy.TestAsync(new DataContract.TestParameters()); 
                });
                                         
                return true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error occured when checking reachability", ex);

                return false;
            }
        }
    }
}