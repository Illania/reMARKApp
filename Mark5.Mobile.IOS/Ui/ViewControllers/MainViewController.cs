//
// Project: Mark5.Mobile.IOS
// File: MainViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.IO;
using System.Linq;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    
    public class MainViewController : UITabBarController, IUITabBarControllerDelegate
    {

        const string SearchTag = "search";
        const string DocumentTag = "document";
        const string ContactTag = "contact";
        const string ShortcodeTag = "shortcode";
        const string NotificationsTag = "notifications";
        const string SettingsTag = "settings";
        
        NavigationController searchNavigationController;
        DocumentsSplitViewController documentSplitViewController;
        ContactsSplitViewController contactSplitViewController;
        ShortcodesSplitViewController shortcodeSplitViewController;
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
            searchNavigationController.Tag = SearchTag;

            documentSplitViewController = new DocumentsSplitViewController();
            documentSplitViewController.TabBarItem.Title = Localization.GetString("documents");
            documentSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "documents.png"));
            documentSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "documents-filled.png"));
            documentSplitViewController.Tag = DocumentTag;

            contactSplitViewController = new ContactsSplitViewController();
            contactSplitViewController.TabBarItem.Title = Localization.GetString("contacts");
            contactSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "contacts.png"));
            contactSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "contacts-filled.png"));
            contactSplitViewController.Tag = ContactTag;

            shortcodeSplitViewController = new ShortcodesSplitViewController();
            shortcodeSplitViewController.TabBarItem.Title = Localization.GetString("shortcodes");
            shortcodeSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "shortcodes.png"));
            shortcodeSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "shortcodes-filled.png"));
            shortcodeSplitViewController.Tag = ShortcodeTag;

            var notificationsListViewController = new NotificationsListViewController();
            notificationsListViewController.TabBarItem.Title = Localization.GetString("notifications");
            notificationsListViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "notifications.png"));
            notificationsListViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "notifications-filled.png"));
            notificationsNavigationController = new NavigationController(notificationsListViewController);
            notificationsNavigationController.Tag = NotificationsTag;

            var settingsViewController = new SettingsViewController();
            settingsViewController.TabBarItem.Title = Localization.GetString("settings");
            settingsViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "settings.png"));
            settingsViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "settings-filled.png"));
            settingsNavigationController = new NavigationController(settingsViewController);
            settingsNavigationController.Tag = SettingsTag;
        }
        
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            Delegate = this;

            var tabsOrder = PlatformConfig.Preferences.MainTabsOrder;
            if (tabsOrder != null && tabsOrder.Length == 6)
            {
                ViewControllers = new ITaggedViewController[]
                {
                    searchNavigationController,
                    documentSplitViewController,
                    contactSplitViewController,
                    shortcodeSplitViewController,
                    notificationsNavigationController,
                    settingsNavigationController
                }.OrderBy(vc => Array.IndexOf(tabsOrder, vc.Tag)).OfType<UIViewController>().ToArray();
            }
            else
            {
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
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            Delegate = null;
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

        [Export("tabBarController:didEndCustomizingViewControllers:changed:")]
        void TabBarControllerDidEndCustomizing(UITabBarController tbc, UIViewController[] vcs, bool changed)
        {
            if (!changed) return;

            PlatformConfig.Preferences.MainTabsOrder = vcs.OfType<ITaggedViewController>().Select(vc => vc.Tag).ToArray();
        }
    }
}
