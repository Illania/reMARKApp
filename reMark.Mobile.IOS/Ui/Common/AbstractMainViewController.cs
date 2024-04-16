using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreGraphics;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.IOS.Common.ShareExtension;
using reMark.Mobile.IOS.Model;
using reMark.Mobile.IOS.Model.HubMessages;
using reMark.Mobile.IOS.Ui.ViewControllers;
using reMark.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using reMark.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
{
    public class AbstractMainViewController : UITabBarController
    {
        protected const string DocumentsTag = "documents";
        protected const string ContactsTag = "contacts";
        protected const string ShortcodesTag = "shortcodes";
        protected const string SettingsTag = "settings";
        protected const string SearchTag = "search";

        protected NavigationModule.NavigationModuleType CurrentNavigationModuleType;

        protected NavigationController SettingsNavigationController;

        UIView navigationButtonContainer;
        UIButtonScalable moduleNavigationButton;

        UIView searchButtonContainer;
        UIView createButtonContainer;
        UIButtonScalable searchButton;
        UIButtonScalable createButton;

        TinyMessageSubscriptionToken reMarkNav;

        #region ShareOptions
        protected bool openedfromSharingOptions = false;
        protected SharingOptions sharingOptions;
        #endregion


        public override void LoadView()
        {
            base.LoadView();

            TabBar.TintColor = Theme.DarkBlue;
            TabBar.UnselectedItemTintColor = Theme.DarkBlue;
            CurrentNavigationModuleType = NavigationModule.NavigationModuleType.Mail;

            SelectedIndex = 1;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewControllerSelected += Handle_ViewControllerSelected;

            ShouldSelectViewController += Handle_ShouldSelectViewController;

            RestorationIdentifier = nameof(AbstractMainViewController);

            SubscribeToMessages();
            navigationButtonContainer = new TouchTransparentView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.AddSubview(navigationButtonContainer);
            View.AddConstraints(new[]
            {
                navigationButtonContainer.HeightAnchor.ConstraintEqualTo(65f),
                navigationButtonContainer.WidthAnchor.ConstraintEqualTo(55f),
                navigationButtonContainer.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                navigationButtonContainer.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : BottomLayoutGuide.TopAnchor, 2),
            });

            moduleNavigationButton = new UIButtonScalable
            {
                TintColor = Theme.White,
                BackgroundColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ImageEdgeInsets = new UIEdgeInsets(15f, 15f, 15f, 15f)
            };

            moduleNavigationButton.SetImage(UIImage.FromBundle("Documents-Filled").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            moduleNavigationButton.Layer.BorderColor = Theme.DarkBlue.CGColor;
            moduleNavigationButton.Layer.BorderWidth = .7f;
            moduleNavigationButton.Layer.CornerRadius = 27.5f;
            navigationButtonContainer.AddSubview(moduleNavigationButton);
            navigationButtonContainer.AddConstraints(new[]
            {
                moduleNavigationButton.HeightAnchor.ConstraintEqualTo(55f),
                moduleNavigationButton.WidthAnchor.ConstraintEqualTo(55f),
                moduleNavigationButton.CenterXAnchor.ConstraintEqualTo(navigationButtonContainer.CenterXAnchor),
                moduleNavigationButton.BottomAnchor.ConstraintEqualTo(navigationButtonContainer.BottomAnchor, -10f),
            });

            searchButtonContainer = new TouchTransparentView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.AddSubview(searchButtonContainer);
            View.AddConstraints(new[]
            {
                searchButtonContainer.HeightAnchor.ConstraintEqualTo(65f),
                searchButtonContainer.WidthAnchor.ConstraintEqualTo(55f),
                searchButtonContainer.LeftAnchor.ConstraintEqualTo(View.LeftAnchor, 10f),
                searchButtonContainer.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : BottomLayoutGuide.TopAnchor, 2),
            });

            searchButton = new UIButtonScalable
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ImageEdgeInsets = new UIEdgeInsets(15f, 15f, 15f, 15f)
            };

            searchButton.SetImage(UIImage.FromBundle("Nav-search").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            searchButtonContainer.AddSubview(searchButton);
            searchButtonContainer.AddConstraints(new[]
            {
                searchButton.HeightAnchor.ConstraintEqualTo(55f),
                searchButton.WidthAnchor.ConstraintEqualTo(55f),
                searchButton.CenterXAnchor.ConstraintEqualTo(searchButtonContainer.CenterXAnchor),
                searchButton.BottomAnchor.ConstraintEqualTo(searchButtonContainer.BottomAnchor),
            });

            createButtonContainer = new TouchTransparentView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.AddSubview(createButtonContainer);
            View.AddConstraints(new[]
            {
                createButtonContainer.HeightAnchor.ConstraintEqualTo(65f),
                createButtonContainer.WidthAnchor.ConstraintEqualTo(55f),
                createButtonContainer.RightAnchor.ConstraintEqualTo(View.RightAnchor, 0),
                createButtonContainer.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : BottomLayoutGuide.TopAnchor, 2),
            });

            createButton = new UIButtonScalable
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ImageEdgeInsets = new UIEdgeInsets(15f, 15f, 15f, 15f)
            };

            createButton.SetImage(UIImage.FromBundle("Compose").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            createButtonContainer.AddSubview(createButton);
            createButtonContainer.AddConstraints(new[]
            {
                createButton.HeightAnchor.ConstraintEqualTo(55f),
                createButton.WidthAnchor.ConstraintEqualTo(55f),
                createButton.CenterXAnchor.ConstraintEqualTo(createButtonContainer.CenterXAnchor),
                createButton.BottomAnchor.ConstraintEqualTo(createButtonContainer.BottomAnchor),
            });

            CustomizableViewControllers = null;

            CommonConfig.MessengerHub.Publish(new NavigationModuleChangedMessage(this, new NavigationModule(CurrentNavigationModuleType)));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            TabBar.Items[2].Enabled = false;

            moduleNavigationButton.TouchUpInside += ModuleNavigationButton_TouchUpInside;
            searchButton.TouchUpInside += SearchButton_TouchUpInside;
            createButton.TouchUpInside += CreateButton_TouchUpInside;

            CustomizableViewControllers = null;
        }

        private void CreateButton_TouchUpInside(object sender, EventArgs e)
        {
  
            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
            
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            OnBoardingUtilities.ShowOnBoardingIfNecessary(this);

            if (openedfromSharingOptions)
                CreateDocumentFromSharingOptions();
            else
                CheckAutoSavedDocument();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            moduleNavigationButton.TouchUpInside -= ModuleNavigationButton_TouchUpInside;
            searchButton.TouchUpInside -= SearchButton_TouchUpInside;
            createButton.TouchUpInside -= CreateButton_TouchUpInside;

        }

        public void SetBottomNavigationButtonsHidden(bool hidden)
        {
            navigationButtonContainer.Hidden = hidden;
            searchButtonContainer.Hidden = hidden;
            createButtonContainer.Hidden = hidden;
        }

        public void SetBottomNavigationButtonsAlpha(float val)
        {
            moduleNavigationButton.Alpha = val;
            searchButton.Alpha = val;
            createButton.Alpha = val;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            View.BringSubviewToFront(navigationButtonContainer);
            View.BringSubviewToFront(searchButtonContainer);
            View.BringSubviewToFront(createButtonContainer);
        }

        protected void CreateDocumentFromSharingOptions()
        {
            if (sharingOptions == null)
                return;

            var sharedText = string.Empty;

            if (sharingOptions.SharedContentInsertType == SharedContentInsertType.Text)
            {
                var textFileUrl = sharingOptions.UrlList.FirstOrDefault();
                sharedText = File.ReadAllText(textFileUrl.Path);
            }

            var vc = new ComposeDocumentViewController(sharingOptions) {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New,
                PreconfiguredContent = string.IsNullOrEmpty(sharedText) ? null : sharedText
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
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

        #region Module Navigation related

        void ModuleNavigationButton_TouchUpInside(object sender, EventArgs e)
        {
            var del = UIApplication.SharedApplication?.Delegate as AppDelegate;
            var root = del?.Window?.RootViewController as AbstractMainViewController;

            var vc = new ModuleNavigationController(CurrentNavigationModuleType);

            if (Integration.IsIPad())
            {
                IPadModalPresentationController customPresentationController = new IPadModalPresentationController(vc, this);
                vc.TransitioningDelegate = customPresentationController;
                vc.ModalPresentationStyle = UIModalPresentationStyle.Custom;
            }

            PresentViewController(vc, true, null);
        }

        void SearchButton_TouchUpInside(object sender, EventArgs e)
        {
            OpenSearch();
        }

        void Handle_ViewControllerSelected(object sender, UITabBarSelectionEventArgs e)
        {
            var x = TabBar.SelectedItem;

            var abVc = (AbstractMainViewController)sender;
            var selectedIndex = abVc.SelectedIndex;

            if (selectedIndex == 0)
                OpenSearch();
            if(selectedIndex == 4)
                OpenSettings();
        }

        bool Handle_ShouldSelectViewController(UITabBarController tabBarController, UIViewController viewController)
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
            var nextModule = obj.Module;
            var nextModuleType = obj.Module.Type;

            if (nextModuleType != NavigationModule.NavigationModuleType.Search)
                CurrentNavigationModuleType = nextModuleType;

            ModuleType module = ModuleType.None;

            switch (nextModuleType)
            {
                case NavigationModule.NavigationModuleType.Mail:
                    moduleNavigationButton.SetImage(UIImage.FromBundle(nextModule.Image).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    SelectedIndex = 0;
                    break;
                case NavigationModule.NavigationModuleType.Contacts:
                    module = ModuleType.Contacts;
                    moduleNavigationButton.SetImage(UIImage.FromBundle(nextModule.Image).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    SelectedIndex = 1;
                    break;
                case NavigationModule.NavigationModuleType.Shortcodes:
                    module = ModuleType.Shortcodes;
                    moduleNavigationButton.SetImage(UIImage.FromBundle(nextModule.Image).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    SelectedIndex = 2;
                    break;
                case NavigationModule.NavigationModuleType.Settings:
                    moduleNavigationButton.SetImage(UIImage.FromBundle(nextModule.Image).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    OpenSettings();
                    break;
                case NavigationModule.NavigationModuleType.Search:
                    OpenSearch();
                    break;
            }

            if (module != ModuleType.None)
                CommonConfig.UsageAnalytics.LogEvent(new OpenModuleEvent(module));
        }

        void OpenSettings()
        {
            var nc = new NavigationController(new SettingsViewController(), UIModalPresentationStyle.FullScreen)
            {
                ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve,
                RestorationIdentifier = "NavigationController_" + nameof(SettingsViewController)
            };
            PresentViewController(nc, true, null);
        }

        void OpenSearch()
        {
            var nc = new DarkNavigationController(new SearchCriteriaViewController(), UIModalPresentationStyle.FullScreen)
            {
                ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve,
                RestorationIdentifier = "NavigationController_" + nameof(SearchCriteriaViewController)
            };
            PresentViewController(nc, true, null);
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
