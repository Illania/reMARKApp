//
// Project: Mark5.Mobile.IOS
// File: UI.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    
    public static class UI
    {
        
        public static UIColor UIColorFromHexString(string hexString)
        {
            try
            {
                float alpha = 255f;
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
    }
}
