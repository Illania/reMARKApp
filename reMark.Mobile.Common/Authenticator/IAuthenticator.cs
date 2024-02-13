using System.Threading;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Classes.Azure;

namespace reMark.Mobile.Common.Authenticator
{
    public interface IAuthenticator
    {
        Task<bool> IsAuthenticatedAsync(CancellationToken ct = default(CancellationToken));

        Task<ConnectionInfo> AuthenticateAsync(string username, string password, SslMode sslMode, string hostname, string port, CancellationToken ct = default(CancellationToken));

        Task<ConnectionInfo> AuthenticateWithAzureAsync(AzureUser azureUser, SslMode sslMode, string hostname, string port, CancellationToken ct = default(CancellationToken),
            string accessToken = "", AzureApplicationProxyInfo azureApplicationProxyInfo = null);

        Task<ConnectionInfo> GetConnectionInfoAsync(CancellationToken ct = default(CancellationToken));

        Task<ConnectionInfo> GetRetainedConnectionInfoAsync(CancellationToken ct = default(CancellationToken));

        Task RetainConnectionInfoAsync(CancellationToken ct = default(CancellationToken));

        Task SaveConnectionInfoAsync(ConnectionInfo connectionInfo, CancellationToken ct = default(CancellationToken));

        Task DeleteRetainedConnectionInfoAsync(CancellationToken ct = default(CancellationToken));
    }
}