namespace Mark5.Mobile.Common.Services
{
    public interface IAbstractService
    {
        void Notify();
        bool IsRunning();
        void Start();
        void Stop();
    }
}