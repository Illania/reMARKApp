//
// Project: Mark5.Mobile.IOS
// File: AbstractMainViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.IO;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
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
                TintColor = Theme.DarkerBlue,
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(14f, 14f, 14f, 14f)
            };
            searchButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "search_large.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            searchButton.Layer.BorderColor = UIColor.FromRGB(167f / 255f, 167f / 255f, 170f / 255f).CGColor;
            searchButton.Layer.BorderWidth = 1f;
            searchButton.Layer.CornerRadius = 27.5f;
            View.AddSubview(searchButton);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 55f),
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 55f),
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

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CheckAutoSavedDocument();
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

        async void CheckAutoSavedDocument()
        {
            try
            {
                var container = await Managers.DocumentsManager.GetAutoSavedDocumentAsync();

                if (container == null)
                    return;

                var title = Localization.GetString("autosave_recover_title");
                var content = Localization.GetString("autosave_recover_content");

                var shouldRecover = await Dialogs.ShowYesNoDialogAsync(this, title, content);
                if (shouldRecover)
                {
                    var vc = new ComposeDocumentViewController
                    {
                        LocalDocument = true,
                        CreationModeFlag = DocumentCreationModeFlag.Edit,
                        PreviousDocumentDirection = container.DocumentPreview.Direction,
                        OutgoingDocumentGuid = container.Info.Identifier
                    };

                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }
                else
                {
                    await Managers.DocumentsManager.DeleteAutoSavedDocumentAsync();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while checking if autosaved document is present", ex);
            }
        }
    }
}