//
// Project: Mark5.Mobile.IOS
// File: MainViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.IO;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    
    public class MainViewController : UITabBarController
    {
        
        NavigationController searchNavigationController;
        DocumentSplitViewController documentSplitViewController;
        ContactSplitViewController contactSplitViewController;
        ShortcodeSplitViewController shortcodeSplitViewController;
        NavigationController notificationsNavigationController;
        NavigationController settingsNavigationController;

        public override void LoadView()
        {
            base.LoadView();

            var searchViewController = new SearchViewController();
            searchViewController.TabBarItem.Title = Localization.GetString("search");
            searchViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "documents.png")); // TODO ICON put correct icon
            searchViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "documents-filled.png")); // TODO ICON put correct icon
            searchNavigationController = new NavigationController(searchViewController);

            documentSplitViewController = new DocumentSplitViewController();
            documentSplitViewController.TabBarItem.Title = Localization.GetString("documents");
            documentSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "documents.png"));
            documentSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "documents-filled.png"));

            contactSplitViewController = new ContactSplitViewController();
            contactSplitViewController.TabBarItem.Title = Localization.GetString("contacts");
            contactSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "contacts.png"));
            contactSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "contacts-filled.png"));

            shortcodeSplitViewController = new ShortcodeSplitViewController();
            shortcodeSplitViewController.TabBarItem.Title = Localization.GetString("shortcodes");
            shortcodeSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "shortcodes.png"));
            shortcodeSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "shortcodes-filled.png"));

            var notificationsListViewController = new NotificationsListViewController();
            notificationsListViewController.TabBarItem.Title = Localization.GetString("notifications");
            notificationsListViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "notifications.png"));
            notificationsListViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "notifications-filled.png"));
            notificationsNavigationController = new NavigationController(notificationsListViewController);

            var settingsViewController = new SettingsViewController();
            settingsViewController.TabBarItem.Title = Localization.GetString("settings");;
            settingsViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "settings.png"));
            settingsViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "settings-filled.png"));
            settingsNavigationController = new NavigationController(settingsViewController);
        }
        
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            WeakDelegate = this;

            ViewControllers = new UIViewController[]
                {
                    searchNavigationController,
                    documentSplitViewController,
                    contactSplitViewController,
                    shortcodeSplitViewController,
                    notificationsNavigationController,
                    settingsNavigationController
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
