using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Storage;
using Mark5.ServiceReference;
using DataContract = Mark5.ServiceReference.DataContract;

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
                await proxy.TestAsync(new DataContract.TestParameters());

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