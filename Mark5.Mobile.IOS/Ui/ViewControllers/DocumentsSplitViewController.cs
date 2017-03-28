//
// Project: Mark5.Mobile.IOS
// File: DocumentsSplitViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class DocumentsSplitViewController : AbstractSplitViewController
    {

        protected override NavigationController CreatePrimaryNavigationController()
        {
            return new NavigationController(new FoldersNotificationsListViewController(ModuleType.Documents));
        }

        protected override NavigationController CreateSecondaryNavigationController()
        {
            return new NavigationController(new DocumentViewController());
        }
    }
}
