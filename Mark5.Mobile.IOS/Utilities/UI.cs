using System;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class UI
    {
        #region Pretty printing

        static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string PrettyFileSize(long bytes)
        {
            try
            {
                if (bytes < 0)
                    return "Unknown size";

                var mag = (int)Math.Log(bytes, 1024);
                var adjustedSize = (decimal)bytes / (1L << (mag * 10));

                return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to get size for {bytes} bytes.", ex);

                return "Unknown size";
            }
        }

        public static string PriorityString(Priority priority)
        {
            switch (priority)
            {
                case Priority.Low:
                    return Localization.GetString("priority_low");
                case Priority.Normal:
                    return Localization.GetString("priority_normal");
                case Priority.Urgent:
                    return Localization.GetString("priority_urgent");
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Color

        public static UIColor UIColorFromHexString(string hexString)
        {
            try
            {
                var alpha = 255f;
                float red, green, blue;

                switch (hexString.Length)
                {
                    case 4: //#RGB
                        red = Convert.ToInt16(hexString.Substring(1, 1), 16);
                        green = Convert.ToInt16(hexString.Substring(2, 1), 16);
                        blue = Convert.ToInt16(hexString.Substring(3, 1), 16);
                        break;
                    case 5: //#ARGB
                        alpha = Convert.ToInt16(hexString.Substring(1, 1), 16);
                        red = Convert.ToInt16(hexString.Substring(2, 1), 16);
                        green = Convert.ToInt16(hexString.Substring(3, 1), 16);
                        blue = Convert.ToInt16(hexString.Substring(4, 1), 16);
                        break;
                    case 7: //#RRGGBB
                        red = Convert.ToInt16(hexString.Substring(1, 2), 16);
                        green = Convert.ToInt16(hexString.Substring(3, 2), 16);
                        blue = Convert.ToInt16(hexString.Substring(5, 2), 16);
                        break;
                    case 9: //#AARRGGBB
                        alpha = Convert.ToInt16(hexString.Substring(1, 1), 16);
                        red = Convert.ToInt16(hexString.Substring(3, 2), 16);
                        green = Convert.ToInt16(hexString.Substring(5, 2), 16);
                        blue = Convert.ToInt16(hexString.Substring(7, 2), 16);
                        break;
                    default:
                        throw new ArgumentException(string.Format("Invalid HEX string passed: {0}.", hexString));
                }

                return UIColor.FromRGBA(red / 255f, green / 255f, blue / 255f, alpha / 255f);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                return UIColor.Clear;
            }
        }

        #endregion

        #region Notifications

        public static float KeyboardHeightFromNotification(NSNotification notification)
        {
            var keyboardFrame = ((NSValue)notification.UserInfo[UIKeyboard.FrameEndUserInfoKey]).CGRectValue;
            return (float)Math.Min(keyboardFrame.Width, keyboardFrame.Height); // Resolving correct dimension, to work around device bug http://stackoverflow.com/questions/9746417/keyboard-willshow-and-willhide-vs-rotation
        }

        public static double KeyboardAnimationDurationFromNotification(NSNotification notification)
        {
            var animationDurationUserInfoKey = notification.UserInfo[UIKeyboard.AnimationDurationUserInfoKey];
            return animationDurationUserInfoKey != null ? ((NSNumber)animationDurationUserInfoKey).DoubleValue : 0.25d;
        }

        public static UIViewAnimationCurve KeyboardAnimationCurveFromNotification(NSNotification notification)
        {
            var animationCurveUserInfoKey = notification.UserInfo[UIKeyboard.AnimationCurveUserInfoKey];
            return animationCurveUserInfoKey != null ? (UIViewAnimationCurve)(int)((NSNumber)animationCurveUserInfoKey).NIntValue : UIViewAnimationCurve.Linear;
        }

        public static UIViewAnimationOptions KeyboardAnimationOptionsFromNotification(NSNotification notification)
        {
            var curve = (int)KeyboardAnimationCurveFromNotification(notification);
            curve |= curve << 16;
            return (UIViewAnimationOptions)curve;
        }

        #endregion
    }
}