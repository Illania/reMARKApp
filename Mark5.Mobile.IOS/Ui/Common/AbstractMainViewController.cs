using System;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.ViewControllers;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class AbstractMainViewController : UITabBarController
    {
        protected const string DocumentsTag = "documents";
        protected const string ContactsTag = "contacts";
        protected const string ShortcodesTag = "shortcodes";
        protected const string SettingsTag = "settings";
        protected const string SearchTag = "search";

        protected NavigationModule.NavigationModuleType CurrentNavigationModule;

        protected NavigationController SearchNavigationController;
        protected NavigationController SettingsNavigationController;

        UIView searchButtonContainer;
        UIButton modalNavigationButton;

        TinyMessageSubscriptionToken reMarkNav;

        public override void LoadView()
        {
            base.LoadView();

            SettingsNavigationController = new NavigationController(new SettingsViewController())
            {
                RestorationIdentifier = "NavigationController_" + nameof(SettingsViewController)
            };

            SettingsNavigationController.TabBarItem.Title = "";

            SearchNavigationController = new NavigationController(new UIViewController());
            SearchNavigationController.TabBarItem.Image = UIImage.FromBundle("Nav-search");
            SearchNavigationController.Tag = SearchTag;
            SearchNavigationController.RestorationIdentifier = "NavigationController_Search";

            TabBar.TintColor = Theme.DarkBlue;
            TabBar.UnselectedItemTintColor = Theme.DarkBlue;
            CurrentNavigationModule = NavigationModule.NavigationModuleType.Mail;

            SelectedIndex = 1;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.ViewControllerSelected += Handle_ViewControllerSelected;

            this.ShouldSelectViewController += Handle_ShouldSelectViewController;

            RestorationIdentifier = nameof(AbstractMainViewController);

            SubscribeToMessages();
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

            modalNavigationButton = new UIButton
            {
                TintColor = Theme.White,
                BackgroundColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(5f, 5f, 5f, 5f)
            };

            modalNavigationButton.SetImage(UIImage.FromBundle("Documents-Filled").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            modalNavigationButton.Layer.BorderColor = Theme.DarkBlue.CGColor;
            modalNavigationButton.Layer.BorderWidth = .7f;
            modalNavigationButton.Layer.CornerRadius = 27.5f;
            searchButtonContainer.AddSubview(modalNavigationButton);
            searchButtonContainer.AddConstraints(new[]
            {
                modalNavigationButton.HeightAnchor.ConstraintEqualTo(55f),
                modalNavigationButton.WidthAnchor.ConstraintEqualTo(55f),
                modalNavigationButton.CenterXAnchor.ConstraintEqualTo(searchButtonContainer.CenterXAnchor),
                modalNavigationButton.BottomAnchor.ConstraintEqualTo(searchButtonContainer.BottomAnchor, -10f),
            });

            CommonConfig.MessengerHub.Publish(new NavigationModuleChangedMessage(this, new NavigationModule(CurrentNavigationModule)));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            TabBar.Items[2].Enabled = false;

            modalNavigationButton.TouchUpInside += ModalNavigationButton_TouchUpInside;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            OnBoardingUtilities.ShowOnBoardingIfNecessary(this);

            CheckAutoSavedDocument();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            modalNavigationButton.TouchUpInside -= ModalNavigationButton_TouchUpInside;
        }

        public void SetSearchButtonHidden(bool hidden)
        {
            searchButtonContainer.Hidden = hidden;
        }

        public void SetSearchButtonAlpha(float val)
        {
            modalNavigationButton.Alpha = val;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            View.BringSubviewToFront(searchButtonContainer);
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
                    CommonConfig.UsageAnalytics.LogEvent(new DocumentRecoveredEvent(true));
                    var vc = new ComposeDocumentViewController { RestoreWorkingCopy = true };
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }
                else
                {
                    CommonConfig.UsageAnalytics.LogEvent(new DocumentRecoveredEvent(false));
                    await Managers.DocumentsManager.DeleteDocumentWorkingCopyAsync();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while checking if document working copy is available!", ex);
            }
        }

        #region Modal Navigation related
        void ModalNavigationButton_TouchUpInside(object sender, EventArgs e)
        {
            var del = UIApplication.SharedApplication?.Delegate as AppDelegate;
            var root = del?.Window?.RootViewController as AbstractMainViewController;
            var selected = root?.SelectedViewController;

            var vc = new ModuleNavigationController(CurrentNavigationModule);

            if (Integration.IsIPad())
            {
                IPadModalPresentationController customPresentationController = new IPadModalPresentationController(vc, this);
                vc.TransitioningDelegate = customPresentationController;
                vc.ModalPresentationStyle = UIModalPresentationStyle.Custom;
            }

            PresentViewController(vc, true, null);
        }

        void Handle_ViewControllerSelected(object sender, UITabBarSelectionEventArgs e)
        {
            var x = TabBar.SelectedItem;

            var abVc = (AbstractMainViewController)sender;
            var selectedIndex = abVc.SelectedIndex;

            if (selectedIndex == 0)
            {
                var nc = new DarkNavigationController(new SearchCriteriaViewController(), UIModalPresentationStyle.FullScreen)
                {
                    ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve,
                    RestorationIdentifier = "NavigationController_" + nameof(SearchCriteriaViewController)
                };
                PresentViewController(nc, true, null);
            }

        }

        private bool Handle_ShouldSelectViewController(UITabBarController tabBarController, UIViewController viewController)
        {

            if (viewController == tabBarController.ViewControllers[0])
            {

                var nc = new DarkNavigationController(new SearchCriteriaViewController(), UIModalPresentationStyle.FullScreen)
                {
                    ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve,
                    RestorationIdentifier = "NavigationController_" + nameof(SearchCriteriaViewController)
                };
                PresentViewController(nc, true, null);

                return false;
            }

            return false;
        }

        void HandleNavigationChangeAction(NavigationModuleChangedMessage obj)
        {
            var nextModule = obj.Module.Type;

            if (nextModule != NavigationModule.NavigationModuleType.Search)
                CurrentNavigationModule = nextModule;

            ModuleType module = ModuleType.None;

            switch (nextModule)
            {
                case NavigationModule.NavigationModuleType.Contacts:
                    module = ModuleType.Contacts;
                    modalNavigationButton.SetImage(UIImage.FromBundle("Nav-contacts").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    SelectedIndex = 2;
                    break;
                case NavigationModule.NavigationModuleType.Calendar:
                    module = ModuleType.Calendar;
                    modalNavigationButton.SetImage(UIImage.FromBundle("Nav-calendar").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    break;
                case NavigationModule.NavigationModuleType.Settings:
                    modalNavigationButton.SetImage(UIImage.FromBundle("Nav-settings").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    SelectedIndex = 4;
                    break;
                case NavigationModule.NavigationModuleType.Shortcodes:
                    module = ModuleType.Shortcodes;
                    modalNavigationButton.SetImage(UIImage.FromBundle("Nav-shortcodes").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    SelectedIndex = 3;
                    break;
                case NavigationModule.NavigationModuleType.Mail:
                    modalNavigationButton.SetImage(UIImage.FromBundle("Nav-mail").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    SelectedIndex = 1;
                    break;
                case NavigationModule.NavigationModuleType.Search:
                    var nc = new DarkNavigationController(new SearchCriteriaViewController(), UIModalPresentationStyle.FullScreen)
                    {
                        ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve,
                        RestorationIdentifier = "NavigationController_" + nameof(SearchCriteriaViewController)
                    };
                    PresentViewController(nc, true, null);
                    break;
            }

            if (module != ModuleType.None)
                CommonConfig.UsageAnalytics.LogEvent(new OpenModuleEvent(module));
        }
        #endregion

        #region Subscribe / unsubscribe

        void SubscribeToMessages()
        {
            reMarkNav = CommonConfig.MessengerHub.Subscribe<NavigationModuleChangedMessage>(HandleNavigationChangeAction);
        }

        void UnsubscribeFromMessages()
        {
            reMarkNav?.Dispose();
        }

        #endregion
    }

    public class IPadModalPresentationController : UIPresentationController, IUIViewControllerTransitioningDelegate
    {

        [Export("presentationControllerForPresentedViewController:presentingViewController:sourceViewController:")]
        public UIPresentationController GetPresentationControllerForPresentedViewController(UIViewController presentedViewController, UIViewController presentingViewController, UIViewController sourceViewController)
        {
            return this;
        }

        UIVisualEffectView blureEffectView;

        public IPadModalPresentationController(UIViewController presentedViewController, UIViewController presentingViewController) : base(presentedViewController, presentingViewController)
        {
            var blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);
            blureEffectView = new UIVisualEffectView(blurEffect);
            blureEffectView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
        }

        public override CGRect FrameOfPresentedViewInContainerView
        {
            get
            {
                return new CGRect(new CGPoint(50, this.ContainerView.Frame.Height / 2 - 200), new CGSize(this.ContainerView.Frame.Width - 100, this.ContainerView.Frame.Height / 2 + 200));
            }
        }

        void Dismiss()
        {
            PresentedViewController.DismissViewController(true, null);
        }

        public override void DismissalTransitionWillBegin()
        {
            base.DismissalTransitionWillBegin();

            PresentedViewController.GetTransitionCoordinator().AnimateAlongsideTransition(
            (UIViewControllerTransitionCoordinatorContext) =>
            {
                blureEffectView.Alpha = 0;
            }, (UIViewControllerTransitionCoordinatorContext) =>
            {
                this.blureEffectView.RemoveFromSuperview();
            });
        }

        public override void PresentationTransitionDidEnd(bool completed)
        {
            base.PresentationTransitionDidEnd(completed);
        }

        public override void PresentationTransitionWillBegin()
        {
            blureEffectView.Alpha = 0;
            ContainerView.AddSubview(blureEffectView);
            PresentedViewController.GetTransitionCoordinator().AnimateAlongsideTransition(
            (UIViewControllerTransitionCoordinatorContext) =>
            {
                blureEffectView.Alpha = 1;
            }, (UIViewControllerTransitionCoordinatorContext) =>
            {

            });
        }

        public override void ContainerViewWillLayoutSubviews()
        {
            base.ContainerViewWillLayoutSubviews();
            PresentedView.Layer.MasksToBounds = true;
            PresentedView.Layer.CornerRadius = 10;
        }

        public override void ContainerViewDidLayoutSubviews()
        {
            base.ContainerViewDidLayoutSubviews();
            PresentedView.Frame = FrameOfPresentedViewInContainerView;
            blureEffectView.Frame = ContainerView.Bounds;
        }
    }
}