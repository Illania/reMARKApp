using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public static class Theme
    {
        public static UIColor TintColor => DarkerBlue;

        #region Nordic IT Colors

        public static UIColor LightBlue => UIColor.FromRGB(199f / 255f, 222f / 255f, 232f / 255f);

        public static UIColor Blue => UIColor.FromRGB(69f / 255f, 133f / 255f, 176f / 255f);

        public static UIColor DarkBlue => UIColor.FromRGB(23f / 255f, 79f / 255f, 107f / 255f);

        public static UIColor DarkerBlue => UIColor.FromRGB(18f / 255f, 61f / 255f, 87f / 255f);

        public static UIColor LightBrown => UIColor.FromRGB(204f / 255f, 192f / 255f, 178f / 255f);

        public static UIColor Brown => UIColor.FromRGB(153f / 255f, 135f / 255f, 107f / 255f);

        public static UIColor White => UIColor.White;

        public static UIColor LightGray => UIColor.FromRGB(227f / 255f, 227f / 255f, 227f / 255f);

        public static UIColor Gray => UIColor.FromRGB(245f / 255f, 245f / 255f, 245f / 255f);

        public static UIColor DarkGray => UIColor.FromRGB(153f / 255f, 153f / 255f, 153f / 255f);

        public static UIColor Black => UIColor.Black;

        #endregion

        #region Fonts

        const string DefaultFontName = "Avenir-Book";
        const string DefaultBoldFontName = "Avenir-Black";
        const string DefaultLightFontName = "Avenir-Light";

        const float DefaultFontSize = 16f;

        public static UIFont DefaultFont => UIFont.FromName(DefaultFontName, DefaultFontSize);

        public static UIFont DefaultBoldFont => UIFont.FromName(DefaultBoldFontName, DefaultFontSize);

        public static UIFont DefaultLightFont => UIFont.FromName(DefaultLightFontName, DefaultFontSize);

        public static UIFont DefaultActionsFont => UIFont.FromName(DefaultFontName, 14f);

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
            UISegmentedControl.Appearance.TintColor = DarkerBlue;

            // UISwitch
            UISwitch.Appearance.OnTintColor = DarkBlue;

            //WKWebView
            WKWebView.Appearance.TintColor = DarkBlue;
        }

        #endregion
    }
}