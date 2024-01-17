using System.Threading;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Classes.Azure;
using reMark.Mobile.Common.Model.Converters;
using reMark.Mobile.Common.Storage;
using reMark.ServiceReference;
using DataContract = reMark.ServiceReference.DataContract;

namespace reMark.Mobile.Common.Authenticator
{
    class Authenticator : IAuthenticator
    {
        public async Task<bool> IsAuthenticatedAsync(CancellationToken ct = default(CancellationToken))
        {
            return await GetConnectionInfoAsync(ct) != null;
        }

        public async Task<ConnectionInfo> AuthenticateAsync(string username, string password, SslMode sslMode, string hostname, string port, CancellationToken ct = default(CancellationToken))
        {
            var deviceType = CommonConfig.DeviceInfoProvider.GetDeviceType();
            var deviceName = CommonConfig.DeviceInfoProvider.GetDeviceName();
            var deviceId = CommonConfig.DeviceInfoProvider.GetDeviceId();

            var proxy = AppServiceProxyFactory.Create(sslMode != SslMode.Off,
                                                      hostname,
                                                      port,
                                                      CommonConfig.HttpClientHandler,
                                                      CommonConfig.OnStartTransmission,
                                                      CommonConfig.OnStopTransmission,
                                                      CommonConfig.Reachability);
            var result = await proxy.AuthenticateAsync(new DataContract.AuthenticateParameters
            {
                Username = username,
                Password = password,
                DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                FriendlyDeviceName = deviceName,
                InstallationId = deviceId
            }, ct);

            var connectionInfo = new ConnectionInfo
            {
                Token = result.Token,
                Username = username,
                Hostname = hostname,
                Port = port,
                SslMode = sslMode,
                DeviceType = deviceType,
                FriendlyDeviceName = deviceName,
                InstallationId = deviceId
            };

            return connectionInfo;
        }

        public async Task<ConnectionInfo> AuthenticateWithAzureAsync(AzureUser azureUser, SslMode sslMode, string hostname, string port,
            CancellationToken ct = default(CancellationToken), string accessToken = "", AzureApplicationProxyInfo azureApplicationProxyInfo = null)
        {
            var deviceType = CommonConfig.DeviceInfoProvider.GetDeviceType();
            var deviceName = CommonConfig.DeviceInfoProvider.GetDeviceName();
            var deviceId = CommonConfig.DeviceInfoProvider.GetDeviceId();

            var proxy = AppServiceProxyFactory.Create(sslMode != SslMode.Off,
                                          hostname,
                                          port,
                                          CommonConfig.HttpClientHandler,
                                          CommonConfig.OnStartTransmission,
                                          CommonConfig.OnStopTransmission,
                                          CommonConfig.Reachability,
                                          accessToken,
                                          azureApplicationProxyInfo);

            var result = await proxy.AuthenticateWithAzureAsync(new DataContract.AuthenticateWithAzureParameters
            {
                AzureUser = azureUser.Convert(),
                DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                FriendlyDeviceName = deviceName,
                InstallationId = deviceId
            }, ct);

            var connectionInfo = new ConnectionInfo
            {
                Token = result.Token,
                Username = result.User?.Username ?? string.Empty,
                AzureUserId = azureUser.Id,
                Hostname = hostname,
                Port = port,
                SslMode = sslMode,
                DeviceType = deviceType,
                FriendlyDeviceName = deviceName,
                InstallationId = deviceId,
                AzureAppProxyBearerToken = accessToken,
                AzureApplicationProxyInfo = azureApplicationProxyInfo
            };

            return connectionInfo;
        }

        public async Task<ConnectionInfo> GetConnectionInfoAsync(CancellationToken ct = default(CancellationToken))
        {
            return await FileSystemStorage.GetConnectionInfoAsync(ct);
        }

        public async Task SaveConnectionInfoAsync(ConnectionInfo connectionInfo, CancellationToken ct = default(CancellationToken))
        {
            await FileSystemStorage.SaveConnectionInfoAsync(connectionInfo, ct);
        }

        public async Task<ConnectionInfo> GetRetainedConnectionInfoAsync(CancellationToken ct = default(CancellationToken))
        {
            return await FileSystemStorage.GetRetainedConnectionInfoAsync(ct);
        }

        public async Task RetainConnectionInfoAsync(CancellationToken ct = default(CancellationToken))
        {
            await FileSystemStorage.RetainConnectionInfoAsync(ct);
            return;
        }

        public async Task DeleteRetainedConnectionInfoAsync(CancellationToken ct = default(CancellationToken))
        {
            await FileSystemStorage.DeleteRetainedConnectionInfoAsync(ct);
            return;
        }
    }
}
