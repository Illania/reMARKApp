//
// Project: Mark5.Mobile.IOS
// File: UI.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Foundation;
using Mark5.Mobile.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class UI
    {
        #region Pretty printing

        static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string PrettyFileSize(long bytes)
        {
            var mag = (int)Math.Log(bytes, 1024);
            decimal adjustedSize = (decimal)bytes / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }

        #endregion

        #region Color

        public static UIColor UIColorFromHexString(string hexString)
        {
            try
            {
                float alpha = 255.0f;
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

                return UIColor.FromRGBA(red / 255.0f, green / 255.0f, blue / 255.0f, alpha / 255.0f);
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
        #endregion

    }
}
