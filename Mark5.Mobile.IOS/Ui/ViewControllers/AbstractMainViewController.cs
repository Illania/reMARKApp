using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Analytics;
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
            searchButton.Layer.BorderWidth = .7f;
            searchButton.Layer.CornerRadius = 27.5f;
            searchButtonContainer.AddSubview(searchButton);
            searchButtonContainer.AddConstraints(new[]
            {
                searchButton.HeightAnchor.ConstraintEqualTo(55f),
                searchButton.WidthAnchor.ConstraintEqualTo(55f),
                searchButton.CenterXAnchor.ConstraintEqualTo(searchButtonContainer.CenterXAnchor),
                searchButton.BottomAnchor.ConstraintEqualTo(searchButtonContainer.BottomAnchor, -10f),
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

        public void SetSearchButtonHidden(bool hidden)
        {
            searchButtonContainer.Hidden = hidden;
        }

        public void SetSearchButtonAlpha(float val)
        {
            searchButton.Alpha = val;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            View.BringSubviewToFront(searchButtonContainer);
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
                    AnalyticsManager.LogEvent(new EmailRecoveredEvent(true));
                    var vc = new ComposeDocumentViewController { RestoreWorkingCopy = true };
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }
                else
                {
                    AnalyticsManager.LogEvent(new EmailRecoveredEvent(false));
                    await Managers.DocumentsManager.DeleteDocumentWorkingCopyAsync();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while checking if document working copy is available!", ex);
            }
        }
    }
}