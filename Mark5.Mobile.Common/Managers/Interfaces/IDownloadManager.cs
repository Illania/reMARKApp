using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{
    public interface IDownloadManager
    {
        DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; }

        Dictionary<ObjectType, DownloadPolicy> DownloadPolicies { get; set; }

        void Notify(ObjectType objectType, int folderId);

        Task<bool> IsRunning();

        Task Start();

        Task Stop();
    }
}

