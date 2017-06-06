using System.Threading;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Tester
{
    public interface ITester
    {
        Task<bool> CanTest(CancellationToken ct = default(CancellationToken));

        Task<bool> Test(CancellationToken ct = default(CancellationToken));
    }
}