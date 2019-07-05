namespace Mark5.Mobile.Common.Presenters
{
    public abstract class BasePresenter<V> : IPresenter<V> where V : IView
    {
        protected V view;

        public void AttachView(V view)
        {
            this.view = view;
        }

        public abstract void Start();
        public abstract void Stop();
    }

    public interface IPresenter<V> where V : IView
    {
        void AttachView(V view);
        void Start();
        void Stop();
    }

    public interface IView
    {

    }
}
