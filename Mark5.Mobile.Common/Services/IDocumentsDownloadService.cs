using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Services
{
    public interface IDocumentsDownloadService
    {
        Task Notify(int folderId);
        Task<bool> IsRunning();
        Task Start();
        Task Stop();
    }
}