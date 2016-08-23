//
// Project: Mark5.Mobile.Common
// File: AuthenticationManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
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
            return (await GetConnectionInfoAsync(ct))?.Authenticated ?? false;
        }

        public async Task<ConnectionInfo> GetConnectionInfoAsync(CancellationToken ct = default(CancellationToken))
        {
            return await FileSystemStorage.GetConnectionInfoAsync(ct);
        }

        public async Task<ConnectionInfo> AuthenticateAsync(string username, string password, SslMode sslMode, string hostname, int port, DeviceType deviceType, string installationId, string friendlyDeviceName, CancellationToken ct = default(CancellationToken))
        {
            var proxy = AppServiceProxyFactory.Create(sslMode != SslMode.Off, hostname, port);
            var result = await proxy.AuthenticateAsync(new DataContract.AuthenticationParameters
            {
                Username = username,
                Password = password,
                DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                FriendlyDeviceName = friendlyDeviceName,
                InstallationId = installationId
            }, ct);

            if (ct.IsCancellationRequested)
            {
                return null;
            }

            var connectionInfo = new ConnectionInfo
            {
                Token = result.Token,
                Username = username,
                Hostname = hostname,
                Port = port,
                SslMode = sslMode,
                InstallationId = installationId,
                DeviceType = deviceType,
                FriendlyDeviceName = friendlyDeviceName,
                Authenticated = true
            };

            await FileSystemStorage.SaveConnectionInfoAsync(connectionInfo, ct);

            return connectionInfo;
        }
    }
}

