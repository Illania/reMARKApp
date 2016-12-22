//
// Project: Mark5.Mobile.IOS
// File: AbstractMainViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class AbstractMainViewController : UITabBarController
    {

        protected const string SearchTag = "search";
        protected const string DocumentTag = "document";
        protected const string ContactTag = "contact";
        protected const string ShortcodeTag = "shortcode";
        protected const string NotificationsTag = "notifications";
        protected const string SettingsTag = "settings";
    }
}
