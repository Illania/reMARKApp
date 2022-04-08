using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
  
    public static class UIFontExtensions{

      
        public static UIFont CustomFont(this UIFont font, UITraitCollection traitCollection = null)
        {
            try
            {
                var textStyle = //Theme.CustomFonts[font];
                                UIFontDescriptor.PreferredBody.TextStyle;

                var scaledFont = font;

                if(traitCollection != null)
                    scaledFont = UIFontMetrics.GetMetrics(textStyle).GetScaledFont(font, traitCollection);
                else
                    scaledFont = UIFontMetrics.GetMetrics(textStyle).GetScaledFont(font);

                return scaledFont;
            }
            catch (Exception)
            {
                return font;
            }
              
        }

    }

    public static class Theme
    {
        public readonly static Dictionary<UIFont, string> CustomFonts = new()
        {
            {
                UIFont.FromName(Theme.DefaultLightFontName, 28f),
                UIFontDescriptor.PreferredTitle1.TextStyle
            },   //AppointmentTitleFont.CustomFont()
            {
                UIFont.FromName(Theme.DefaultFontName, 22f),
                UIFontDescriptor.PreferredTitle2.TextStyle
            },
            {
                UIFont.FromName(Theme.DefaultFontName, 18f),
                UIFontDescriptor.PreferredTitle3.TextStyle
            },        //AppointmentDefaultFont.CustomFont().CustomFont()
            {
                UIFont.FromName(Theme.DefaultBoldFontName, 16f),
                UIFontDescriptor.PreferredHeadline.TextStyle
            },  //DefaultBoldFont.CustomFont()Name
            {
                UIFont.FromName(Theme.DefaultFontName, 16f),
                UIFontDescriptor.PreferredBody.TextStyle
            },          //DefaultFont.CustomFont()
            {
                UIFont.FromName(Theme.DefaultLightFontName, 16f),
                UIFontDescriptor.PreferredCallout.TextStyle
            },  //DefaultLightFont.CustomFont()
            {
                UIFont.FromName(Theme.DefaultFontName, 14f),
                UIFontDescriptor.PreferredSubheadline.TextStyle
            },   //DefaultActionsFont.CustomFont()
            {
                UIFont.FromName(Theme.DefaultLightFontName, 14f),
                UIFontDescriptor.PreferredFootnote.TextStyle
            }, //CalendarTimeLightFont.CustomFont()
            {
                UIFont.FromName(Theme.DefaultLightFontName, 12f),
                UIFontDescriptor.PreferredCaption1.TextStyle
            },
            {
                UIFont.FromName(Theme.DefaultLightFontName, 11f),
                UIFontDescriptor.PreferredCaption2.TextStyle
            }
        };

         

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
        public static UIColor Bookmark => UIColor.FromRGB(98, 66, 38);

        #endregion

        #region Fonts

        //not scalable fonts
        const float DefaultFontXSmallSize = 8f;
        const float DefaultFontSmallSize = 10f;
        public static UIFont CalendarFontXSmall => UIFont.FromName(DefaultLightFontName, DefaultFontXSmallSize);
        public static UIFont CalendarFontSmall => UIFont.FromName(DefaultLightFontName, DefaultFontSmallSize);

        public const string DefaultFontName = "Avenir-Book";
        public const string DefaultBoldFontName = "Avenir-Black";
        public const string DefaultLightFontName = "Avenir-Light";

        public static UIFont AppointmentTitleFont => UIFont.FromName(DefaultLightFontName, 28f);
        public static UIFont AppointmentDefaultFont => UIFont.FromName(DefaultFontName, 18f);
        public static UIFont DefaultBoldFont => UIFont.FromName(DefaultBoldFontName, 16f);
        public static UIFont DefaultFont => UIFont.FromName(DefaultFontName, 16f);
        public static UIFont DefaultLightFont => UIFont.FromName(DefaultLightFontName, 16f);
        public static UIFont DefaultActionsFont => UIFont.FromName(DefaultFontName, 14f);
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
                Font = DefaultFont.CustomFont().WithRelativeSize(1f)
            };

            if (Integration.IsRunningAtLeast(11))
            {
                UINavigationBar.Appearance.LargeTitleTextAttributes = new UIStringAttributes
                {
                    ForegroundColor = DarkerBlue,
                    Font = DefaultFont.CustomFont().WithRelativeSize(12f)
                };
                UINavigationBar.AppearanceWhenContainedIn(typeof(DarkNavigationController)).LargeTitleTextAttributes = new UIStringAttributes
                {
                    ForegroundColor = LightGray,
                    Font = DefaultFont.CustomFont().WithRelativeSize(12f)
                };
            }

            UINavigationBar.AppearanceWhenContainedIn(typeof(DarkNavigationController)).TintColor = LightGray;
            UINavigationBar.AppearanceWhenContainedIn(typeof(DarkNavigationController)).SetBackgroundImage(SolidColorImage(DarkerBlue), UIBarMetrics.Default);
            UINavigationBar.AppearanceWhenContainedIn(typeof(DarkNavigationController)).TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = LightGray,
                Font = DefaultFont.CustomFont().WithRelativeSize(1f)
            };

            UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.CustomFont()
            }, UIControlState.Normal);
            UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.CustomFont()
            }, UIControlState.Highlighted);
            UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                TextColor = DarkGray,
                Font = DefaultFont.CustomFont()
            }, UIControlState.Disabled);

            UITabBarItem.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.CustomFont().WithRelativeSize(-5f)
            }, UIControlState.Normal);

            UISegmentedControl.Appearance.TintColor = DarkerBlue;
            UISegmentedControl.Appearance.SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.CustomFont().WithRelativeSize(-4f)
            }, UIControlState.Normal);

            UISegmentedControl.AppearanceWhenContainedIn(typeof(DarkNavigationController)).TintColor = DarkBlue;
            UISegmentedControl.AppearanceWhenContainedIn(typeof(DarkNavigationController)).SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.CustomFont().WithRelativeSize(-4f),
                TextColor = White
            }, UIControlState.Normal);
            UISegmentedControl.AppearanceWhenContainedIn(typeof(DarkNavigationController)).SetTitleTextAttributes(new UITextAttributes
            {
                Font = DefaultFont.CustomFont().WithRelativeSize(-4f),
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
            try
            {
                if (view is UITableViewHeaderFooterView header)
                {
                    if (header?.TextLabel == null)
                        return;
                    var text = header.TextLabel?.Text ?? string.Empty;
                    header.TextLabel.Text = null;
                    header.TextLabel.AttributedText = new NSAttributedString(text, new UIStringAttributes { Font = DefaultLightFont.CustomFont(), ForegroundColor = DarkerBlue });
                    return;
                }

                if (view is UIButtonScalable button)
                {
                    button.TitleLabel.Font = DefaultFont.CustomFont();
                    return;
                }

                if (view is UITextFieldScalable textField)
                {
                    textField.Font = DefaultFont.CustomFont();
                    return;
                }

                if (view is UITextViewScalable textView)
                {
                    textView.Font = DefaultFont.CustomFont();
                    return;
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Can't apply theme to header view or view type {view.GetType().Name}.", ex);
            }
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