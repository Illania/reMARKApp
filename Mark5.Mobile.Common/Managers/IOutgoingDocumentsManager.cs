using System;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common
{
    public interface IOutgoingDocumentsManager
    {
        void Notify(Guid identifier);

        Task<bool> IsRunning();

        Task Start();

        Task Stop();
    }
}

