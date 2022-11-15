using System;

namespace FastScrollRecycler
{
    public class ActionOnFastScrollStateChangeListener : IOnFastScrollStateChangeListener
    {
        readonly Action startAction;
        readonly Action stopAction;

        public ActionOnFastScrollStateChangeListener(Action startAction = null, Action stopAction = null)
        {
            this.startAction = startAction;
            this.stopAction = stopAction;
        }

        public void OnFastScrollStart()
        {
            startAction?.Invoke();
        }

        public void OnFastScrollStop()
        {
            stopAction?.Invoke();
        }
    }
}