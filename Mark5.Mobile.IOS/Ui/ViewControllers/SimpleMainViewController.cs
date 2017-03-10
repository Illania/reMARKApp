//
// Project: Mark5.Mobile.IOS
// File: SimpleMainViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.IO;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class SimpleMainViewController : AbstractMainViewController, IUITabBarControllerDelegate
    {

        NavigationController searchNavigationController;
        NavigationController documentsNavigationController;
        NavigationController contactsNavigationController;
        NavigationController shortcodesNavigationController;
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

            documentsNavigationController = new NavigationController(new BrowseFoldersListViewController(ModuleType.Documents));
            documentsNavigationController.TabBarItem.Title = Localization.GetString("documents");
            documentsNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "documents.png"));
            documentsNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "documents-filled.png"));
            documentsNavigationController.Tag = DocumentTag;

            contactsNavigationController = new NavigationController(new BrowseFoldersListViewController(ModuleType.Contacts));
            contactsNavigationController.TabBarItem.Title = Localization.GetString("contacts");
            contactsNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "contacts.png"));
            contactsNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "contacts-filled.png"));
            contactsNavigationController.Tag = ContactTag;

            shortcodesNavigationController = new NavigationController(new BrowseFoldersListViewController(ModuleType.Shortcodes));
            shortcodesNavigationController.TabBarItem.Title = Localization.GetString("shortcodes");
            shortcodesNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "shortcodes.png"));
            shortcodesNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "shortcodes-filled.png"));
            shortcodesNavigationController.Tag = ShortcodeTag;

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

            Delegate = this;

            var tabsOrder = PlatformConfig.Preferences.MainTabsOrder;
            if (tabsOrder != null && tabsOrder.Length == 6)
            {
                ViewControllers = new ITaggedViewController[]
                {
                    //searchNavigationController,
                    documentsNavigationController,
                    contactsNavigationController,
                    shortcodesNavigationController,
                    notificationsNavigationController,
                    settingsNavigationController
                }.OrderBy(vc => Array.IndexOf(tabsOrder, vc.Tag)).OfType<UIViewController>().ToArray();

                var indexOfDocuments = Array.IndexOf(ViewControllers, documentsNavigationController);
                if (indexOfDocuments < 0) indexOfDocuments = 0;
                SelectedIndex = indexOfDocuments;
            }
            else
            {
                ViewControllers = new UIViewController[]
                {
                    //searchNavigationController,
                    documentsNavigationController,
                    contactsNavigationController,
                    shortcodesNavigationController,
                    notificationsNavigationController,
                    settingsNavigationController
                };

                SelectedIndex = 0;
            }
        }

        [Export("tabBarController:didEndCustomizingViewControllers:changed:")]
        public void FinishedCustomizingViewControllers2(UITabBarController tbc, UIViewController[] vcs, bool changed)
        {
            if (!changed) return;

            PlatformConfig.Preferences.MainTabsOrder = vcs.OfType<ITaggedViewController>().Select(vc => vc.Tag).ToArray();
        }
    }
}
