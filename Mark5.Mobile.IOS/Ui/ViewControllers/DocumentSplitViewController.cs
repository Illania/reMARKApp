//
// Project: Mark5.Mobile.IOS
// File: DocumentSplitViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class DocumentSplitViewController : SplitViewController
    {

        public DocumentSplitViewController()
        {
            PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;
            ViewControllers = new UIViewController[]
            {
                new NavigationController(new BrowseFoldersListViewController(ModuleType.Documents)),
                new NavigationController(new DocumentViewController())
            };

            CollapseSecondViewController = (splitViewController, secondaryViewController, primaryViewController) => true;
        }
    }
}
