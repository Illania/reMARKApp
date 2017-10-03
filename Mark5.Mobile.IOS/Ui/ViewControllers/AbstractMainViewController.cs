using System;
using System.IO;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class AbstractMainViewController : UITabBarController
    {
        protected const string DocumentsTag = "documents";
        protected const string ContactsTag = "contacts";
        protected const string ShortcodesTag = "shortcodes";
        protected const string SettingsTag = "settings";

        protected UINavigationController Dummy { get; } = new UINavigationController(new UIViewController());

        UIView searchButtonContainer;
        UIButton searchButton;
        NSLayoutConstraint searchButtonBottomConstraint;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            searchButtonContainer = new TouchTransparentView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(searchButtonContainer);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(searchButtonContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 65f),
                NSLayoutConstraint.Create(searchButtonContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 55f),
                NSLayoutConstraint.Create(searchButtonContainer, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(searchButtonContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View.SafeAreaLayoutGuide, NSLayoutAttribute.Bottom, 1f, 2f)
            });

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
            searchButtonContainer.AddSubview(searchButton);
            searchButtonContainer.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 55f),
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 55f),
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, searchButtonContainer, NSLayoutAttribute.CenterX, 1f, 0f),
                searchButtonBottomConstraint = NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, searchButtonContainer, NSLayoutAttribute.Bottom, 1f, -10f)
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

        public void SetSearchButtonHidden(bool hidden, bool animated)
        {
            searchButtonBottomConstraint.Constant = hidden ? -10f : 0f;

            if (animated)
                UIView.AnimateNotify(.25d, 0d, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseOut, searchButtonContainer.LayoutIfNeeded, null);
            else
                searchButtonContainer.LayoutIfNeeded();
        }

        void SearchButton_TouchUpInside(object sender, EventArgs e)
        {
            var nc = new DarkNavigationController(new SearchCriteriaViewController(), UIModalPresentationStyle.FullScreen)
            {
                ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve,
                RestorationIdentifier = "NavigationController_" + nameof(SearchCriteriaViewController)
            };
            PresentViewController(nc, true, null);
        }

        async void CheckAutoSavedDocument()
        {
            try
            {
                var isAvailable = await Managers.DocumentsManager.IsDocumentWorkingCopyAvailableAsync();
                if (!isAvailable)
                    return;

                var shouldRecover = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("autosave_recover_title"), Localization.GetString("autosave_recover_content"));
                if (shouldRecover)
                {
                    var vc = new ComposeDocumentViewController { RestoreWorkingCopy = true };
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }
                else
                    await Managers.DocumentsManager.DeleteDocumentWorkingCopyAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while checking if document working copy is available!", ex);
            }
        }
    }
}