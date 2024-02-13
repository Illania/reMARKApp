using System.Threading.Tasks;

namespace reMark.Mobile.Common.Job
{
    public interface IJob
    {
        Task Run();
    }
}
