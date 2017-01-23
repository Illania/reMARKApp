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

        protected BrowseFoldersListViewController(Folder folder)
            : base(folder, false, false, false)
        {
        }

        protected override void FolderSelected(Folder folder)
        {
            base.FolderSelected(folder);

            if (folder.Module == ModuleType.Documents)
            {
                var vc = new DocumentsListViewController { Folder = folder };
                NavigationController.PushViewController(vc, true);
            }

            if (folder.Module == ModuleType.Contacts)
            {
                var vc = new ContactsListViewController { Folder = folder };
                NavigationController.PushViewController(vc, true);
            }

            if (folder.Module == ModuleType.Shortcodes)
            {
                var vc = new ShortcodesListViewController { Folder = folder };
                NavigationController.PushViewController(vc, true);
            }
        }

        protected override void FolderExpand(Folder folder)
        {
            base.FolderExpand(folder);

            var vc = new BrowseFoldersListViewController(folder);
            NavigationController.PushViewController(vc, true);
        }
    }
}
