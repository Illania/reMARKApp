using System;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public static class Theme
    {
        public static UIColor TintColor => DarkerBlue;

        #region Nordic IT Colors

        public static UIColor OpaqueLightGray => LightGray.ColorWithAlpha(0.5f);
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
        public static UIColor Clear => UIColor.Clear;

        #endregion

        #region Fonts

        const string DefaultFontName = "Avenir-Book";
        const string DefaultBoldFontName = "Avenir-Black";
        const string DefaultLightFontName = "Avenir-Light";
        const string DefaultLightBoldFontName = "Avenir-Medium";

        const float DefaultFontSize = 16f;

        public static UIFont DefaultFont => UIFont.FromName(DefaultFontName, DefaultFontSize);
        public static UIFont DefaultBoldFont => UIFont.FromName(DefaultBoldFontName, DefaultFontSize);
        public static UIFont DefaultLightFont => UIFont.FromName(DefaultLightFontName, DefaultFontSize);
        public static UIFont DefaultActionsFont => UIFont.FromName(DefaultFontName, 14f);
        public static UIFont DefaultLightBoldFont => UIFont.FromName(DefaultLightBoldFontName, DefaultFontSize);

        public static UIFont AppointmentTitleFont => UIFont.FromName(DefaultLightFontName, 28f);
        public static UIFont AppointmentDefaultFont => UIFont.FromName(DefaultFontName, DefaultFontSize + 2);
        public static UIFont CalendarTimeLightFont => UIFont.FromName(DefaultLightFontName, 14f);

        #endregion

        #region Cells

        public const float MinimumLabelSize = 18f;

        #endregion

        #region Apply theme methods

        public static void ApplyTheme(this UIWindow window)
        {
            window.TintColor = TintColor;

            UINavigationBar.Appearance.TintColor = DarkerBlue;
            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = DarkerBlue,
                Font = DefaultFont.WithRelativeSize(1f)
            };

            if (Integration.IsRunningAtLeast(11))
            {
                UINavigationBar.Appearance.LargeTitleTextAttributes = new UIStringAttributes
                {
                    ForegroundColor = DarkerBlue,
                    Font = DefaultFont.WithRelativeSize(12f)
                };
                UINavigationBar.AppearanceWhenContainedIn(typeof(DarkNavigationController)).LargeTitleTextAttributes = new UIStringAttributes
                {
                    ForegroundColor = LightGray,
                    Font = DefaultFont.WithRelativeSize(12f)
                };
            }

            UINavigationBar.AppearanceWhenContainedIn(typeof(DarkNavigationController)).TintColor = LightGray;
            UINavigationBar.AppearanceWhenContainedIn(typeof(DarkNavigationController)).SetBackgroundImage(SolidColorImage(DarkerBlue), UIBarMetrics.Default);
            UINavigationBar.AppearanceWhenContainedIn(typeof(DarkNavigationController)).TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = LightGray,
                Font = DefaultFont.WithRelativeSize(1f)
            };

            UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont
            }, UIControlState.Normal);
            UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont
            }, UIControlState.Highlighted);
            UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                TextColor = DarkGray,
                Font = DefaultFont
            }, UIControlState.Disabled);

            UITabBarItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.WithRelativeSize(-5f)
            }, UIControlState.Normal);

            UISegmentedControl.Appearance.TintColor = DarkerBlue;
            UISegmentedControl.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.WithRelativeSize(-4f)
            }, UIControlState.Normal);

            UISegmentedControl.AppearanceWhenContainedIn(typeof(DarkNavigationController)).TintColor = DarkBlue;
            UISegmentedControl.AppearanceWhenContainedIn(typeof(DarkNavigationController)).SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.WithRelativeSize(-4f),
                TextColor = White
            }, UIControlState.Normal);
            UISegmentedControl.AppearanceWhenContainedIn(typeof(DarkNavigationController)).SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.WithRelativeSize(-4f),
                TextColor = White
            }, UIControlState.Selected);

            UIProgressView.Appearance.TintColor = DarkerBlue;
            UIProgressView.AppearanceWhenContainedIn(typeof(DarkNavigationController)).TintColor = Blue;

            UISwitch.Appearance.OnTintColor = DarkBlue;

            UIRefreshControl.Appearance.TintColor = LightGray;

            UITableViewHeaderFooterView.Appearance.TintColor = LightGray;

            WKWebView.Appearance.TintColor = DarkBlue;
        }

        public static void ApplyTheme(this UIView view)
        {
            if (view is UITableViewHeaderFooterView header)
            {
                var text = header.TextLabel.Text ?? string.Empty;
                header.TextLabel.Text = null;
                header.TextLabel.AttributedText = new NSAttributedString(text, new UIStringAttributes { Font = DefaultLightFont, ForegroundColor = DarkerBlue });
                return;
            }

            if (view is UIButton button)
            {
                button.TitleLabel.Font = DefaultFont;
                return;
            }

            if (view is UITextField textField)
            {
                textField.Font = DefaultFont;
                return;
            }

            if (view is UITextView textView)
            {
                textView.Font = DefaultFont;
                return;
            }

            throw new ArgumentException($"No theme found for view type {view.GetType().Name}!");
        }

        #endregion

        #region Utilities

        static UIImage SolidColorImage(UIColor color)
        {
            var rect = new CGRect(0f, 0f, 1f, 1f);
            UIGraphics.BeginImageContext(rect.Size);
            color.ColorWithAlpha(0.99f).SetFill();
            UIGraphics.RectFill(rect);
            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image.CreateResizableImage(UIEdgeInsets.Zero, UIImageResizingMode.Stretch);
        }

        #endregion

    }
}