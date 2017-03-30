//
// Project: Mark5.Mobile.IOS
// File: AbstractMainViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.IO;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class AbstractMainViewController : UITabBarController
    {

        protected const string DocumentTag = "document";
        protected const string ContactTag = "contact";
        protected const string ShortcodeTag = "shortcode";
        protected const string SettingsTag = "settings";

        protected UINavigationController Dummy { get; } = new UINavigationController(new UIViewController());

        UIButton searchButton;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            searchButton = new UIButton
            {
                TintColor = Theme.LightBlue,
                BackgroundColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(12f, 12f, 12f, 12f)
            };
            searchButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "search_large.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            searchButton.Layer.CornerRadius = 25f;
            View.AddSubview(searchButton);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 50f),
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 50f),
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, -8f),
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1f, 0f)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            TabBar.Items[2].Enabled = false;

            searchButton.TouchUpInside += SearchButton_TouchUpInside;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            searchButton.TouchUpInside -= SearchButton_TouchUpInside;
        }

        void SearchButton_TouchUpInside(object sender, System.EventArgs e)
        {
            var nc = new NavigationController(new SearchCriteriaViewController(), UIModalPresentationStyle.FullScreen)
            {
                ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve
            };
            PresentViewController(nc, true, null);
        }
    }
}
