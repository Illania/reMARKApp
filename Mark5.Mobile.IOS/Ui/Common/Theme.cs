//
// Project: Mark5.Mobile.IOS
// File: Theme.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public static class Theme
    {

        public static UIColor TintColor
        {
            get
            {
                return Blue;
            }
        }

        #region Nordic IT Colors

        public static UIColor DarkBlue
        {
            get
            {
                return UIColor.FromRGB(8.0f / 255.0f, 51.0f / 255.0f, 77.0f / 255.0f);
            }
        }

        public static UIColor Blue
        {
            get
            {
                return UIColor.FromRGB(18.0f / 255.0f, 61.0f / 255.0f, 87.0f / 255.0f);
            }
        }

        public static UIColor LightBlue
        {
            get
            {
                return UIColor.FromRGB(69.0f / 255.0f, 133.0f / 255.0f, 176.0f / 255.0f);
            }
        }

        public static UIColor Brown
        {
            get
            {
                return UIColor.FromRGB(153.0f / 255.0f, 135.0f / 255.0f, 107.0f / 255.0f);
            }
        }

        public static UIColor LightBrown
        {
            get
            {
                return UIColor.FromRGB(204.0f / 255.0f, 192.0f / 255.0f, 178.0f / 255.0f);
            }
        }

        public static UIColor White
        {
            get
            {
                return UIColor.White;
            }
        }

        public static UIColor LightGray
        {
            get
            {
                return UIColor.FromRGB(227.0f / 255.0f, 227.0f / 255.0f, 227.0f / 255.0f);
            }
        }

        public static UIColor LighterGray
        {
            get
            {
                return UIColor.FromRGB(245.0f / 255.0f, 245.0f / 255.0f, 245.0f / 255.0f);
            }
        }

        public static UIColor Black
        {
            get
            {
                return UIColor.Black;
            }
        }

        public static UIColor Gray
        {
            get
            {
                return UIColor.FromRGB(150.0f / 255.0f, 150.0f / 255.0f, 150.0f / 255.0f);
            }
        }

        public static UIColor Red
        {
            get
            {
                return UIColor.Red;
            }
        }

        #endregion

        #region Fonts

        const string DefaultFontName = "Avenir-Book";
        const string DefaultBoldFontName = "Avenir-Black";
        const string DefaultLightFontName = "Avenir-Light";

        const float DefaultFontSize = 16.0f;

        public static UIFont DefaultFont
        {
            get
            {
                return UIFont.FromName(DefaultFontName, DefaultFontSize);
            }
        }

        public static UIFont DefaultBoldFont
        {
            get
            {
                return UIFont.FromName(DefaultBoldFontName, DefaultFontSize);
            }
        }

        public static UIFont DefaultLightFont
        {
            get
            {
                return UIFont.FromName(DefaultLightFontName, DefaultFontSize);
            }
        }

        public static UIFont DefaultDocumentFont
        {
            get
            {
                return UIFont.SystemFontOfSize(16.0f);
            }
        }

        public static UIFont DefaultOutgoingDocumentFont
        {
            get
            {
                return UIFont.SystemFontOfSize(12.0f);
            }
        }

        #endregion

        #region Apply theme methods

        public static void ApplyTheme(UIWindow window)
        {
            // Color customizations
            // ////////////////////

            window.TintColor = TintColor;

            // UINavigationBar
            UINavigationBar.Appearance.TintColor = LightGray;
            UINavigationBar.Appearance.BarTintColor = DarkBlue;

            // UIToolBar
            UIToolbar.Appearance.BarTintColor = LightGray;

            // UITabBar
            UITabBar.Appearance.BarTintColor = LightGray;

            // UITableViewHeaderFooterView
            UITableViewHeaderFooterView.Appearance.TintColor = LightGray;

            // Font customizations
            // ///////////////////

            // UINavigationBar
            var uiNavigationBarTitleTextAttributes = UINavigationBar.Appearance.GetTitleTextAttributes();
            uiNavigationBarTitleTextAttributes.TextColor = LightGray;
            uiNavigationBarTitleTextAttributes.Font = DefaultBoldFont.WithRelativeSize(1.0f);
            UINavigationBar.Appearance.SetTitleTextAttributes(uiNavigationBarTitleTextAttributes);

            // UIBarItem
            var uiBarItemTitleTextAttributes = UIBarItem.Appearance.GetTitleTextAttributes(UIControlState.Normal);
            uiBarItemTitleTextAttributes.TextColor = LightGray;
            uiBarItemTitleTextAttributes.Font = DefaultFont.WithRelativeSize(1.0f);
            UIBarItem.Appearance.SetTitleTextAttributes(uiBarItemTitleTextAttributes, UIControlState.Normal);

            // UILabel
            UILabel.Appearance.Font = DefaultFont;

            // UIBarButtonItem
            var uiBarButtonItemTitleTextAttributes = UIBarButtonItem.Appearance.GetTitleTextAttributes(UIControlState.Normal);
            uiBarItemTitleTextAttributes.TextColor = Blue;
            uiBarButtonItemTitleTextAttributes.Font = DefaultFont;
            UIBarButtonItem.Appearance.SetTitleTextAttributes(uiBarButtonItemTitleTextAttributes, UIControlState.Normal);

            // UITabBarItem
            var uiTabBarItemTitleTextAttributes = UITabBarItem.Appearance.GetTitleTextAttributes(UIControlState.Normal);
            uiTabBarItemTitleTextAttributes.Font = DefaultFont.WithRelativeSize(-5.0f);
            UITabBarItem.Appearance.SetTitleTextAttributes(uiTabBarItemTitleTextAttributes, UIControlState.Normal);

            // UISegmentedControl
            var uiSegmentControlTitleTextAttributes = UIBarButtonItem.Appearance.GetTitleTextAttributes(UIControlState.Normal);
            uiSegmentControlTitleTextAttributes.Font = DefaultFont;
            UISegmentedControl.Appearance.SetTitleTextAttributes(uiSegmentControlTitleTextAttributes, UIControlState.Normal);
        }

        #endregion

    }
}
