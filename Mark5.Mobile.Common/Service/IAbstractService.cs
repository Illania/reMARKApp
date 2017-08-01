namespace Mark5.Mobile.Common.Service
{
    public interface IAbstractService
    {
        void Notify();
        bool IsRunning();
        void Start();
        void Stop(bool allowRestart = false);
    }
}