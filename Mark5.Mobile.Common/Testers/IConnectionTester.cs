using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Testers
{
    public interface IConnectionTester
    {
        Task<bool> CanTest(CancellationToken ct = default(CancellationToken));

        Task<bool> Test(CancellationToken ct = default(CancellationToken));

        Task<ConnectionDiagnosticModel> ConnectionDiagnostics(CancellationToken ct = default(CancellationToken));
    }
}