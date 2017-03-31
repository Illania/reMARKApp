//
// Project: Mark5.Mobile.IOS
// File: Theme.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public static class Theme
    {

        public static UIColor TintColor { get { return DarkerBlue; } }

        #region Nordic IT Colors

        public static UIColor LightBlue { get { return UIColor.FromRGB(199f / 255f, 222f / 255f, 232f / 255f); } }

        public static UIColor Blue { get { return UIColor.FromRGB(69f / 255f, 133f / 255f, 176f / 255f); } }

        public static UIColor DarkBlue { get { return UIColor.FromRGB(23f / 255f, 79f / 255f, 107f / 255f); } }

        public static UIColor DarkerBlue { get { return UIColor.FromRGB(18f / 255f, 61f / 255f, 87f / 255f); } }

        public static UIColor LightBrown { get { return UIColor.FromRGB(204f / 255f, 192f / 255f, 178f / 255f); } }
        
        public static UIColor Brown { get { return UIColor.FromRGB(153f / 255f, 135f / 255f, 107f / 255f); } }

        public static UIColor White { get { return UIColor.White; } }

        public static UIColor LightGray { get { return UIColor.FromRGB(227f / 255f, 227f / 255f, 227f / 255f); } }

        public static UIColor Gray { get { return UIColor.FromRGB(245f / 255f, 245f / 255f, 245f / 255f); } }

        public static UIColor DarkGray { get { return UIColor.FromRGB(153f / 255f, 153f / 255f, 153f / 255f); } }

        public static UIColor Black { get { return UIColor.Black; } }

        #endregion

        #region Fonts

        const string DefaultFontName = "Avenir-Book";
        const string DefaultBoldFontName = "Avenir-Black";
        const string DefaultLightFontName = "Avenir-Light";

        const float DefaultFontSize = 16f;

        public static UIFont DefaultFont { get { return UIFont.FromName(DefaultFontName, DefaultFontSize); } }

        public static UIFont DefaultBoldFont { get { return UIFont.FromName(DefaultBoldFontName, DefaultFontSize); } }

        public static UIFont DefaultLightFont { get { return UIFont.FromName(DefaultLightFontName, DefaultFontSize); } }

        #endregion

        #region Apply theme methods

        public static void ApplyTheme(UIWindow window)
        {
            // Color customizations

            window.TintColor = TintColor;

            // UINavigationBar
            UINavigationBar.Appearance.Translucent = false;
            UINavigationBar.Appearance.TintColor = LightGray;
            UINavigationBar.Appearance.BarTintColor = DarkerBlue;

            // UIToolBar
            UIToolbar.Appearance.BarTintColor = LightGray;

            // UITabBar
            UITabBar.Appearance.BarTintColor = LightGray;

            // UITableViewHeaderFooterView
            UITableViewHeaderFooterView.Appearance.TintColor = LightGray;

            // Font customizations

            // UINavigationBar
            var uiNavigationBarTitleTextAttributes = UINavigationBar.Appearance.GetTitleTextAttributes();
            uiNavigationBarTitleTextAttributes.TextColor = LightGray;
            uiNavigationBarTitleTextAttributes.Font = DefaultBoldFont.WithRelativeSize(1f);
            UINavigationBar.Appearance.SetTitleTextAttributes(uiNavigationBarTitleTextAttributes);

            // UIBarItem
            var uiBarItemTitleTextAttributes = UIBarItem.Appearance.GetTitleTextAttributes(UIControlState.Normal);
            uiBarItemTitleTextAttributes.TextColor = LightGray;
            uiBarItemTitleTextAttributes.Font = DefaultFont.WithRelativeSize(1f);
            UIBarItem.Appearance.SetTitleTextAttributes(uiBarItemTitleTextAttributes, UIControlState.Normal);

            // UILabel
            UILabel.Appearance.Font = DefaultFont;

            // UIBarButtonItem
            var uiBarButtonItemTitleTextAttributes = UIBarButtonItem.Appearance.GetTitleTextAttributes(UIControlState.Normal);
            uiBarItemTitleTextAttributes.TextColor = White;
            uiBarButtonItemTitleTextAttributes.Font = DefaultFont;
            UIBarButtonItem.Appearance.SetTitleTextAttributes(uiBarButtonItemTitleTextAttributes, UIControlState.Normal);

            // UITabBarItem
            var uiTabBarItemTitleTextAttributes = UITabBarItem.Appearance.GetTitleTextAttributes(UIControlState.Normal);
            uiTabBarItemTitleTextAttributes.Font = DefaultFont.WithRelativeSize(-5f);
            UITabBarItem.Appearance.SetTitleTextAttributes(uiTabBarItemTitleTextAttributes, UIControlState.Normal);

            // UISegmentedControl
            var uiSegmentControlTitleTextAttributes = UISegmentedControl.Appearance.GetTitleTextAttributes(UIControlState.Normal);
            uiSegmentControlTitleTextAttributes.Font = DefaultFont;
            UISegmentedControl.Appearance.SetTitleTextAttributes(uiSegmentControlTitleTextAttributes, UIControlState.Normal);

            UISwitch.Appearance.OnTintColor = Theme.DarkBlue;
        }

        #endregion

    }
}
