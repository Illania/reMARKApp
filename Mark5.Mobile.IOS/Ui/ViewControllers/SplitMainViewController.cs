//
// Project: Mark5.Mobile.IOS
// File: SplitMainViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.IO;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    
    public class SplitMainViewController : AbstractMainViewController
    {
        
        NavigationController searchNavigationController;
        DocumentsSplitViewController documentSplitViewController;
        ContactsSplitViewController contactSplitViewController;
        ShortcodesSplitViewController shortcodeSplitViewController;
        NavigationController notificationsNavigationController;
        NavigationController settingsNavigationController;

        public override void LoadView()
        {
            base.LoadView();

            searchNavigationController = new NavigationController(new SearchViewController());
            searchNavigationController.TabBarItem.Title = Localization.GetString("search");
            searchNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "documents.png")); // TODO ICON put correct icon
            searchNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "documents-filled.png")); // TODO ICON put correct icon
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

            notificationsNavigationController = new NavigationController(new NotificationsListViewController());
            notificationsNavigationController.TabBarItem.Title = Localization.GetString("notifications");
            notificationsNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "notifications.png"));
            notificationsNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "notifications-filled.png"));
            notificationsNavigationController.Tag = NotificationsTag;

            settingsNavigationController = new NavigationController(new SettingsViewController());
            settingsNavigationController.TabBarItem.Title = Localization.GetString("settings");
            settingsNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "settings.png"));
            settingsNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "settings-filled.png"));
            settingsNavigationController.Tag = SettingsTag;
        }
        
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ViewControllers = new UIViewController[]
            {
                searchNavigationController,
                documentSplitViewController,
                contactSplitViewController,
                shortcodeSplitViewController,
                notificationsNavigationController,
                settingsNavigationController
            };

            SelectedIndex = 1;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            ViewControllers = null;
        }
    }
}
