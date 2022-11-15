using System;
using Android.Animation;

namespace FastScrollRecycler
{
    class ActionAnimatorListenerAdapter : AnimatorListenerAdapter
    {
        readonly Action actionCancel;
        readonly Action actionEnd;

        public ActionAnimatorListenerAdapter(Action actionCancel = null, Action actionEnd = null)
        {
            this.actionCancel = actionCancel;
            this.actionEnd = actionEnd;
        }

        public override void OnAnimationCancel(Animator animation)
        {
            base.OnAnimationCancel(animation);

            actionCancel?.Invoke();
        }

        public override void OnAnimationEnd(Animator animation)
        {
            base.OnAnimationEnd(animation);

            actionEnd?.Invoke();
        }
    }
}