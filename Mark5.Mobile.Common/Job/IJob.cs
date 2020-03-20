using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Job
{
    public interface IJob
    {
        Task Run();
    }
}
