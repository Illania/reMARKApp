namespace reMark.Mobile.Common.Service
{
    public interface IAbstractService
    {
        void Notify();
        bool IsRunning();
        void Start();
        void Stop(bool allowRestart = false);
    }
}