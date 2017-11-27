using Android.Support.Design.Widget;
using Android.Views;
using Java.Interop;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class FloatingActionButtonBehavior : CoordinatorLayout.Behavior
    {
        public override bool OnStartNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View directTargetChild, View target, int axes, int type)
        {
            var fab = child.JavaCast<FloatingActionButton>();

            return axes == (int) ScrollAxis.Vertical && fab.Visibility != ViewStates.Visible;
        }

        public override void OnNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View target, int dxConsumed, int dyConsumed, int dxUnconsumed, int dyUnconsumed, int type)
        {
            base.OnNestedScroll(coordinatorLayout, child, target, dxConsumed, dyConsumed, dxUnconsumed, dyUnconsumed, type);

            var fab = child.JavaCast<FloatingActionButton>();

            if (dyConsumed > 1)
                fab.Show();
        }
    }
}