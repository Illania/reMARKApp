//
// Project: Mark5.Mobile.Common
// File: IAuthenticationManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Authenticator
{

    public interface IAuthenticator
    {

        Task<bool> IsAuthenticatedAsync(CancellationToken ct = default(CancellationToken));

        Task<ConnectionInfo> GetConnectionInfoAsync(CancellationToken ct = default(CancellationToken));

        Task<ConnectionInfo> AuthenticateAsync(string username, string password, bool ssl, string hostname, int port, DeviceType deviceType, string installationId, string friendlyDeviceName, CancellationToken ct = default(CancellationToken));
    }
}

