using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Storage;
using Mark5.ServiceReference;
using DataContract = Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Authenticator
{
    class Authenticator : IAuthenticator
    {
        public async Task<bool> IsAuthenticatedAsync(CancellationToken ct = default(CancellationToken))
        {
            return await GetConnectionInfoAsync(ct) != null;
        }

        public async Task<ConnectionInfo> AuthenticateAsync(string username, string password, SslMode sslMode, string hostname, int port, CancellationToken ct = default(CancellationToken))
        {
            var deviceType = CommonConfig.DeviceInfoProvider.GetDeviceType();
            var deviceName = CommonConfig.DeviceInfoProvider.GetDeviceName();
            var deviceId = CommonConfig.DeviceInfoProvider.GetDeviceId();

            var proxy = AppServiceProxyFactory.Create(sslMode != SslMode.Off,
                                                      hostname,
                                                      port,
                                                      CommonConfig.HttpClientHandler,
                                                      CommonConfig.OnStartTransmission,
                                                      CommonConfig.OnStopTransmission);
            var result = await proxy.AuthenticateAsync(new DataContract.AuthenticateParameters
            {
                Username = username,
                Password = password,
                DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                FriendlyDeviceName = deviceName,
                InstallationId = deviceId
            },
                ct);

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

    }
}