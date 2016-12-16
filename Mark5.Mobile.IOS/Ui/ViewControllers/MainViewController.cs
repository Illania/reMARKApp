//
// Project: Mark5.Mobile.IOS
// File: MainViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class MainViewController : UITabBarController
    {

        public override void LoadView()
        {
            base.LoadView();

            documentSplitViewController = new DocumentsSplitViewController();
            documentSplitViewController.TabBarItem.Title = "Documents";
            documentSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("Icons", "documents.png"));
            documentSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("Icons", "documents-filled.png"));

            contactSplitViewController = new ContactsSplitViewController();
            contactSplitViewController.TabBarItem.Title = "Contacts";
            contactSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("Icons", "contacts.png"));
            contactSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("Icons", "contacts-filled.png"));

            shortcodeSplitViewController = new ShortcodesSplitViewController();
            shortcodeSplitViewController.TabBarItem.Title = "Shortcodes";
            shortcodeSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("Icons", "shortcodes.png"));
            shortcodeSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("Icons", "shortcodes-filled.png"));

            notificationsListViewController = new NotificationsListViewController();
            notificationsListViewController.TabBarItem.Title = "Notifications";
            notificationsListViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("Icons", "notifications.png"));
            notificationsListViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("Icons", "notifications-filled.png"));
            notificationsNavigationController = new CustomUINavigationController(notificationsListViewController);

            settingsViewController = new SettingsViewController();
            settingsViewController.TabBarItem.Title = "Settings";
            settingsViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("Icons", "settings.png"));
            settingsViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("Icons", "settings-filled.png"));
            settingsNavigationController = new CustomUINavigationController(settingsViewController);
        }
        
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            WeakDelegate = this;

            ViewControllers = new UIViewController[]
                {
                    documentSplitViewController,
                    contactSplitViewController,
                    shortcodeSplitViewController,
                    notificationsNavigationController,
                    settingsNavigationController,
                };
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            WeakDelegate = null;
            ViewControllers = null;
        }

        [Export("tabBarController:didSelectViewController:")]
        void TabBarControllerDidSelectViewController(UITabBarController tbc, UIViewController vc)
        {
            Title = vc.Title;

            if (vc.NavigationItem.LeftBarButtonItem != null) NavigationItem.SetLeftBarButtonItem(vc.NavigationItem.LeftBarButtonItem, false);
            else if (vc.NavigationItem.LeftBarButtonItems != null) NavigationItem.SetLeftBarButtonItems(vc.NavigationItem.LeftBarButtonItems, false);
            else NavigationItem.SetLeftBarButtonItem(null, false);

            if (vc.NavigationItem.RightBarButtonItem != null) NavigationItem.SetRightBarButtonItem(vc.NavigationItem.RightBarButtonItem, false);
            else if (vc.NavigationItem.RightBarButtonItems != null) NavigationItem.SetRightBarButtonItems(vc.NavigationItem.RightBarButtonItems, false);
            else NavigationItem.SetRightBarButtonItem(null, false);
        }
    }
}
