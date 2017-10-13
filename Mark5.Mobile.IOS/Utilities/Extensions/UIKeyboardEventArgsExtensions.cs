using UIKit;

namespace Mark5.Mobile.IOS.Utilities.Extensions
{
    public static class UIKeyboardEventArgsExtensions
    {
        public static UIViewAnimationOptions GetAimationOptions(this UIKeyboardEventArgs e)
        {
            return (UIViewAnimationOptions)((int)e.AnimationCurve << 16);
        }
    }
}