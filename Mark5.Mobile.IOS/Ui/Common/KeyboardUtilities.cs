using System;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public static class KeyboardUtilities
    {
        public static float KeyboardHeightFromNotification(NSNotification notification)
        {
            var keyboardFrame = ((NSValue) notification.UserInfo[UIKeyboard.FrameEndUserInfoKey]).CGRectValue;
            return (float) Math.Min(keyboardFrame.Width, keyboardFrame.Height); // Resolving correct dimension, to work around device bug http://stackoverflow.com/questions/9746417/keyboard-willshow-and-willhide-vs-rotation
        }

        public static double KeyboardAnimationDurationFromNotification(NSNotification notification)
        {
            var animationDurationUserInfoKey = notification.UserInfo[UIKeyboard.AnimationDurationUserInfoKey];
            return animationDurationUserInfoKey != null ? ((NSNumber) animationDurationUserInfoKey).DoubleValue : 0.25d;
        }

        public static UIViewAnimationCurve KeyboardAnimationCurveFromNotification(NSNotification notification)
        {
            var animationCurveUserInfoKey = notification.UserInfo[UIKeyboard.AnimationCurveUserInfoKey];
            return animationCurveUserInfoKey != null ? (UIViewAnimationCurve) (int) ((NSNumber) animationCurveUserInfoKey).NIntValue : UIViewAnimationCurve.Linear;
        }

        public static UIViewAnimationOptions KeyboardAnimationOptionsFromNotification(NSNotification notification)
        {
            var curve = (int) KeyboardAnimationCurveFromNotification(notification);
            curve |= curve << 16;
            return (UIViewAnimationOptions) curve;
        }
    }
}