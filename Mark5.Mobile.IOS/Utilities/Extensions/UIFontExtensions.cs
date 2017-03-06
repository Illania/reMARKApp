//
// Project: Mark5.Mobile.IOS
// File: UIFontExtensions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using UIKit;

namespace Mark5.Mobile.IOS.Utilities.Extensions
{
    
    public static class UIFontExtensions
    {

        public static UIFont WithRelativeSize(this UIFont font, float relativePoints)
        {
            return font.WithSize(font.PointSize + relativePoints);
        }
    }
}
