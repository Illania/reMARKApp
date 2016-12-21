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

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList.BrowseFoldersListViewController"/> class.
        /// </summary>
        /// <param name="module">Module.</param>
        public BrowseFoldersListViewController(ModuleType module)
            : base(module)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList.BrowseFoldersListViewController"/> class.
        /// 
        /// This constructor MUST NOT be public!
        /// </summary>
        /// <param name="folder">Folder.</param>
        protected BrowseFoldersListViewController(Folder folder)
            : base(folder)
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
