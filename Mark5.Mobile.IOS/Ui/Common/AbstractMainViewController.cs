using System;
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
    public class AbstractMainViewController : UITabBarController, AbstractMainViewController.IAbstractMainViewControllerDelegate
    {
        protected const string DocumentsTag = "documents";
        protected const string ContactsTag = "contacts";
        protected const string ShortcodesTag = "shortcodes";
        protected const string SettingsTag = "settings";
        protected const string SearchTag = "search";

        NavigationController documentsNavigationController;
        NavigationController contactsNavigationController;
        NavigationController shortcodesNavigationController;
        NavigationController settingsNavigationController;
        NavigationController SearchDummy;

        UIView searchButtonContainer;
        UIButton searchButton;

        TinyMessageSubscriptionToken reMarkNav;

        public override void LoadView()
        {
            base.LoadView();

            documentsNavigationController = new NavigationController(new FoldersNotificationsListViewController(ModuleType.Documents))
            {
                RestorationIdentifier = "NavigationController_" + nameof(FoldersNotificationsListViewController) + "_" + nameof(ModuleType.Documents)
            };

            contactsNavigationController = new NavigationController(new BrowseFoldersListViewController(ModuleType.Contacts))
            {
                RestorationIdentifier = "NavigationController_" + nameof(BrowseFoldersListViewController) + "_" + nameof(ModuleType.Contacts)
            };

            shortcodesNavigationController = new NavigationController(new BrowseFoldersListViewController(ModuleType.Shortcodes))
            {
                RestorationIdentifier = "NavigationController_" + nameof(BrowseFoldersListViewController) + "_" + nameof(ModuleType.Shortcodes)
            };

            settingsNavigationController = new NavigationController(new SettingsViewController())
            {
                RestorationIdentifier = "NavigationController_" + nameof(SettingsViewController)
            };

            SearchDummy = new NavigationController(new UIViewController());

            SearchDummy.TabBarItem.Title = "Search";
            SearchDummy.TabBarItem.Image = UIImage.FromBundle("Documents");
            SearchDummy.TabBarItem.SelectedImage = UIImage.FromBundle("Documents-Filled");
            SearchDummy.Tag = SearchTag;
            SearchDummy.RestorationIdentifier = "NavigationController_Search";

            ViewControllers = new UIViewController[]
            {
                SearchDummy,
                documentsNavigationController,
                contactsNavigationController,
                shortcodesNavigationController,
                settingsNavigationController
            };

            SelectedIndex = 1;
        }

        void SubscribeToMessages()
        {
            reMarkNav = CommonConfig.MessengerHub.Subscribe<ReMarkNav>(HandleNavigationChangeAction);
        }

        void HandleNavigationChangeAction(ReMarkNav obj)
        {
            var nextModule = obj.Module.Type;
            switch (nextModule)
            {
                case NavigationModule.NavigationModuleType.Contacts:
                    if (SelectedIndex != 2)
                        SelectedIndex = 2;
                    break;
                case NavigationModule.NavigationModuleType.Calendar:

                    break;
                case NavigationModule.NavigationModuleType.Settings:
                    if (SelectedIndex != 4)
                        SelectedIndex = 4;
                    break;
                case NavigationModule.NavigationModuleType.Shortcodes:
                    if (SelectedIndex != 3)
                        SelectedIndex = 3;
                    break;
                case NavigationModule.NavigationModuleType.Mail:
                    if (SelectedIndex != 1)
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
        }

        void UnsubscribeFromMessages()
        {
            reMarkNav?.Dispose();
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

            searchButton = new UIButton
            {
                TintColor = Theme.White,
                BackgroundColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(14f, 14f, 14f, 14f)
            };

            searchButton.SetImage(UIImage.FromBundle("Documents-Filled").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
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

            return true;
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


        void ViewControllerSelected1(object sender, UITabBarSelectionEventArgs e)
        {
            ModuleType module = ModuleType.None;
            if (e.ViewController == documentsNavigationController)
                module = ModuleType.Documents;
            if (e.ViewController == contactsNavigationController)
                module = ModuleType.Contacts;
            if (e.ViewController == shortcodesNavigationController)
                module = ModuleType.Shortcodes;

            if (module != ModuleType.None)
                CommonConfig.UsageAnalytics.LogEvent(new OpenModuleEvent(module));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            TabBar.Items[2].Enabled = false;

            searchButton.TouchUpInside += SearchButton_TouchUpInside;

            ViewControllerSelected += ViewControllerSelected1;
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

            searchButton.TouchUpInside -= SearchButton_TouchUpInside;


            ViewControllerSelected -= ViewControllerSelected1;
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
            var del = UIApplication.SharedApplication?.Delegate as AppDelegate;
            var root = del?.Window?.RootViewController as AbstractMainViewController;
            var selected = root?.SelectedViewController;

            //searchButton.SetImage(UIImage.FromBundle("Failed").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            PresentViewController(new ModuleNavigationController(), false, null);
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

        public void NavigationChanged(UIViewController controller)
        {
            Console.WriteLine("YES");
        }

        public interface IAbstractMainViewControllerDelegate
        {
            void NavigationChanged(UIViewController controller);
        }

    }
}