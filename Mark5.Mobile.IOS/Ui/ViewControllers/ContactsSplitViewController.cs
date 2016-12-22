//
// Project: Mark5.Mobile.IOS
// File: ContactSplitViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    
    public class ContactsSplitViewController : AbstractSplitViewController
    {

        protected override NavigationController CreatePrimaryNavigationController()
        {
            return new NavigationController(new BrowseFoldersListViewController(ModuleType.Contacts));
        }

        protected override NavigationController CreateSecondaryNavigationController()
        {
            return new NavigationController(new DocumentViewController());
        }
    }
}
