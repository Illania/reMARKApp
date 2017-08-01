using System.Threading;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Testers
{
    public interface IConnectionTester
    {
        Task<bool> CanTest(CancellationToken ct = default(CancellationToken));

        Task<bool> Test(CancellationToken ct = default(CancellationToken));
    }
}