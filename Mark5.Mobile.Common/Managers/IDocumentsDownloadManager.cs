using System.Threading.Tasks;

namespace Mark5.Mobile.Common
{
    public interface IDocumentsDownloadManager
    {
        Task Notify(int folderId);
        Task<bool> IsRunning();
        Task Start();
        Task Stop();
    }
}