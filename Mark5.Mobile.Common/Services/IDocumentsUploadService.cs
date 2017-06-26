namespace Mark5.Mobile.Common.Services
{
    public interface IDocumentsUploadService
    {
        void Notify();
        bool IsRunning();
        void Start();
        void Stop();
    }
}