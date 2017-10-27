using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Utilities;
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
                searchButtonContainer.HeightAnchor.ConstraintEqualTo(65f),
                searchButtonContainer.WidthAnchor.ConstraintEqualTo(55f),
                searchButtonContainer.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                searchButtonContainer.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : BottomLayoutGuide.GetTopAnchor(), 2),
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
            searchButton.Layer.BorderColor = Theme.DarkGray.CGColor;
            searchButton.Layer.BorderWidth = 1f;
            searchButton.Layer.CornerRadius = 27.5f;
            searchButtonContainer.AddSubview(searchButton);
            searchButtonContainer.AddConstraints(new[]
            {
                searchButton.HeightAnchor.ConstraintEqualTo(55f),
                searchButton.WidthAnchor.ConstraintEqualTo(55f),
                searchButton.CenterXAnchor.ConstraintEqualTo(searchButtonContainer.CenterXAnchor),
                searchButtonBottomConstraint = searchButton.BottomAnchor.ConstraintEqualTo(searchButtonContainer.BottomAnchor, -10f),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            TabBar.Items[2].Enabled = false;

            ViewControllerSelected += AbstractMainViewController_ViewControllerSelected;
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

            ViewControllerSelected -= AbstractMainViewController_ViewControllerSelected;
            searchButton.TouchUpInside -= SearchButton_TouchUpInside;
        }

        void AbstractMainViewController_ViewControllerSelected(object sender, UITabBarSelectionEventArgs e)
        {
            var nc = e.ViewController as UINavigationController;
            if (nc == null)
                return;

            SetSearchButtonHidden(nc.ToolbarHidden, true);
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

                var shouldRecover = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("autosave_recover_title"), Localization.GetString("autosave_recover_content"));
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