//
// Project: Mark5.Mobile.IOS
// File: BrowseFoldersListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{

    public class BrowseFoldersListViewController : AbstractFoldersListViewController
    {

        public BrowseFoldersListViewController(ModuleType module)
            : base(module, false, false, false)
        {
        }

        /// <summary>
        /// This constructor MUST NOT be public!
        /// </summary>
        protected BrowseFoldersListViewController(Folder folder)
            : base(folder, false, false, false)
        {
        }

        protected override void FolderSelected(Folder folder)
        {
            base.FolderSelected(folder);
        }

        protected override void FolderExpand(Folder folder)
        {
            base.FolderExpand(folder);

            var vc = new BrowseFoldersListViewController(folder);
            NavigationController.PushViewController(vc, true);
        }
    }
}
