using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Storage;
using Mark5.ServiceReference;
using DataContract = Mark5.ServiceReference.DataContract;
using Polly;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mark5.Mobile.Common.Testers
{
    public class ConnectionTester : IConnectionTester
    {
        private readonly double timeOut = 200;

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
                                                          null,
                                                          ci.AzureAppProxyBearerToken);

                var policy = Policy.Handle<Exception>().WaitAndRetryAsync(attempts, attempt => TimeSpan.FromMilliseconds(timeOut), (exception, calculatedWaitDuration) =>
                {
                    retries++;
                    CommonConfig.Logger.Info($"Failed to Ping server after waiting {calculatedWaitDuration}. Retry number {retries}.");
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

        async Task<long> MakeRequest(ConnectionInfo ci, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var proxy = AppServiceProxyFactory.Create(ci.SslMode != SslMode.Off,
                                                          ci.Hostname,
                                                          ci.Port,
                                                          CommonConfig.HttpClientHandler,
                                                          null,
                                                          null);

                var stopWatch = Stopwatch.StartNew();
                var result = await proxy.TestAsync(new DataContract.TestParameters());
                return stopWatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error occured when checking reachability", ex);
                return -1;
            }
        }

        public async Task<ConnectionDiagnosticModel> ConnectionDiagnostics(CancellationToken ct = default(CancellationToken))
        {
            var ci = await FileSystemStorage.GetConnectionInfoAsync(ct);

            if (ci == null)
            {
                CommonConfig.Logger.Error("No configuration file available to test connection");
                return new ConnectionDiagnosticModel(ConnectionDiagnosticModel.ErrorCode.NoConfigurationInfo);
            }

            const int requestsCount = 5;
            ConnectionDiagnosticModel connectionDiagnosticModel = null;

            var taskList = new List<Task<long>>();

            for (int i = 0; i < requestsCount; i++)
            {
                taskList.Add(MakeRequest(ci, ct));
            }

            try
            {
                // run tasks in parallel
                var tasks = Task.WhenAll(taskList.ToArray());
                await tasks.ContinueWith((elapsedTime) =>
                {
                    List<long> ellapsedTimeList = elapsedTime.Result.OfType<long>().ToList();
                    List<long> successResultList = ellapsedTimeList.Where(d => d > -1).ToList();
                    int successResultCount = successResultList.Count();
                    int failedResultCount = ellapsedTimeList.Count - successResultList.Count();
                    long totalEllapsedTime = successResultList.Sum();
                    connectionDiagnosticModel = new ConnectionDiagnosticModel(successResultCount, failedResultCount, totalEllapsedTime);
                });
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Exception occured while testing connection to server", ex);
                connectionDiagnosticModel = new ConnectionDiagnosticModel(ConnectionDiagnosticModel.ErrorCode.UncaughtException);
            }

            return connectionDiagnosticModel;
        }
    }
}
