//
// Project: Mark5.Mobile.IOS
// File: Localization.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    
    public static class Localization
    {

        public static string GetString(string key) => NSBundle.MainBundle.LocalizedString(key, key);
    }
}
