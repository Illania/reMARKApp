using System;
using System.Threading;
using System.Threading.Tasks;
using reMark.Mobile.Classes.Model;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.Testers
{
    public interface IConnectionTester
    {
        Task<bool> CanTest(CancellationToken ct = default(CancellationToken));

        Task<bool> Test(CancellationToken ct = default(CancellationToken));

        Task<ConnectionDiagnosticModel> ConnectionDiagnostics(CancellationToken ct = default(CancellationToken));
    }
}