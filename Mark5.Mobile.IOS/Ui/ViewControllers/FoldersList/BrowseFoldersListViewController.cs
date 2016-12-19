//
// Project: Mark5.Mobile.IOS
// File: BrowseFoldersListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{

    public class BrowseFoldersListViewController : AbstractFoldersListViewController
    {

        public BrowseFoldersListViewController(ModuleType module)
            : base(module)
        {
        }
        
        protected override void FolderSelected(Folder folder)
        {
            // TODO
        }

        protected override void FolderDeselected(Folder folder)
        {
            // TODO
        }
    }
}
