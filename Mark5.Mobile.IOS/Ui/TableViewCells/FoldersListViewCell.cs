//
// Project: Mark5.Mobile.IOS
// File: FoldersListViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    
    public partial class FoldersListViewCell : UITableViewCell
    {
    
        public static readonly UINib Nib = UINib.FromName("FoldersListViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("FoldersListViewCell");

        Folder folder;

        public event EventHandler<Folder> ExpandCollapseClicked;

        public static FoldersListViewCell Create()
        {
            var cell = (FoldersListViewCell)Nib.Instantiate(null, null)[0];
            cell.ExpandCollapseButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "expand.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            cell.OfflineIndicatorImage.Image = UIImage.FromBundle(Path.Combine("Icons", "offline.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            cell.OfflineIndicatorImage.TintColor = Theme.DarkBlue;
            return cell;
        }

        public FoldersListViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public void Initialize(Folder folder)
        {
            this.folder = folder;

            // TODO
            FolderNameLabel.Text = folder.Name;
        }

        partial void ExpandCollapseButtonTouchUpInside(NSObject sender)
        {
            if (ExpandCollapseClicked != null && folder != null)
                ExpandCollapseClicked(this, folder);
        }
    }
}
