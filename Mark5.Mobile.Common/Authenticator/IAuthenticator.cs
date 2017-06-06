using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Authenticator
{
    public interface IAuthenticator
    {
        Task<bool> IsAuthenticatedAsync(CancellationToken ct = default(CancellationToken));

        Task<ConnectionInfo> AuthenticateAsync(string username, string password, SslMode sslMode, string hostname, int port, CancellationToken ct = default(CancellationToken));

        Task<ConnectionInfo> GetConnectionInfoAsync(CancellationToken ct = default(CancellationToken));

        Task SaveConnectionInfoAsync(ConnectionInfo connectionInfo, CancellationToken ct = default(CancellationToken));
    }
}