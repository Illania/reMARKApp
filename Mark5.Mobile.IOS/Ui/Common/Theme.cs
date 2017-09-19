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
            window.TintColor = TintColor;

            UINavigationBar.Appearance.Translucent = false;
            UINavigationBar.Appearance.TintColor = LightGray;
            UINavigationBar.Appearance.BarTintColor = DarkerBlue;
            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = LightGray,
                Font = DefaultFont.WithRelativeSize(1f)
            };
            UINavigationBar.Appearance.LargeTitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = LightGray,
                Font = DefaultFont.WithRelativeSize(10f)
            };

            UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                TextColor = LightGray,
                Font = DefaultFont
            }, UIControlState.Normal);
            UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                TextColor = LightGray,
                Font = DefaultFont
            }, UIControlState.Highlighted);
            UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                TextColor = DarkBlue,
                Font = DefaultFont
            }, UIControlState.Disabled);

            UIToolbar.Appearance.BarTintColor = LightGray;

            UITabBar.Appearance.BarTintColor = LightGray;

            UITabBarItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.WithRelativeSize(-5f)
            }, UIControlState.Normal);

            UISegmentedControl.Appearance.TintColor = DarkerBlue;
            UISegmentedControl.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont
            }, UIControlState.Normal);

            UISwitch.Appearance.OnTintColor = DarkBlue;

            UILabel.Appearance.Font = DefaultFont;

            UIRefreshControl.Appearance.TintColor = LightGray;

            UITableViewHeaderFooterView.Appearance.TintColor = LightGray;

            WKWebView.Appearance.TintColor = DarkBlue;
        }

        #endregion
    }
}