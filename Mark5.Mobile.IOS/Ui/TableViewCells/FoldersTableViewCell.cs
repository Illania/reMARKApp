//
// Project: Mark5.Mobile.IOS
// File: FoldersTableViewCell.cs
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
    
    public partial class FoldersTableViewCell : UITableViewCell
    {
    
        public static readonly UINib Nib = UINib.FromName("FoldersTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("FoldersTableViewCell");

        Folder folder;

        public event EventHandler<Folder> ExpandCollapseClicked;

        public static FoldersTableViewCell Create()
        {
            var cell = (FoldersTableViewCell)Nib.Instantiate(null, null)[0];
            cell.FolderCheckedIndicatorImage.Image = UIImage.FromBundle(Path.Combine("Icons", "checkmark.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            cell.FolderCheckedIndicatorImage.TintColor = Theme.Brown;
            cell.OfflineIndicatorImage.Image = UIImage.FromBundle(Path.Combine("Icons", "offline.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            cell.OfflineIndicatorImage.TintColor = Theme.DarkBlue;
            cell.ExpandCollapseButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "expand.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            return cell;
        }

        public FoldersTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public void Initialize(Folder folder, bool sectionIsFavorites, bool folderIsOffline)
        {
            this.folder = folder;

            FolderNameLabel.Text = folder.Name;
            FolderIconImage.Image = GetIcon(folder);

            if (folder.Subscribed)
            {
                FolderIconImage.TintColor = Theme.Brown;
                FolderCheckedIndicatorImage.TintColor = Theme.Brown;
                FolderCheckedIndicatorImage.Alpha = 1f;
            }
            else
            {
                FolderIconImage.TintColor = Theme.TintColor;
                FolderCheckedIndicatorImage.TintColor = Theme.TintColor;
                FolderCheckedIndicatorImage.Alpha = 0f;
            }

            if (folderIsOffline)
            {
                OfflineIndicatorLeadingConstraint.Constant = 10f;
                OfflineIndicatorWidthConstraint.Constant = 15f;
            }
            else
            {
                OfflineIndicatorLeadingConstraint.Constant = 0f;
                OfflineIndicatorWidthConstraint.Constant = 0f;
            }

            if (folder.HasSubFolders)
            {
                ExpandCollapseButton.Alpha = 1f;
                ExpandCollapseButton.Hidden = false;
            }
            else
            {
                ExpandCollapseButton.Alpha = 0f;
                ExpandCollapseButton.Hidden = true;
            }
        }

        partial void ExpandCollapseButtonTouchUpInside(NSObject sender)
        {
            if (ExpandCollapseClicked != null && folder != null)
                ExpandCollapseClicked(this, folder);
        }

        static UIImage GetIcon(Folder folder)
        {
            if (folder.Id == Folder.DocumentsOutgoingFolder.Id)
            {
                return UIImage.FromBundle(Path.Combine("icons", "folderslist", "outgoing.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            if (folder.InternalType == FolderInternalType.Worktray)
            {
                return UIImage.FromBundle(Path.Combine("icons", "folderslist", "worktray.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            if (folder.Type == FolderType.Inbox)
            {
                return UIImage.FromBundle(Path.Combine("icons", "folderslist", "inbox.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            if (folder.Type == FolderType.Outbox)
            {
                return UIImage.FromBundle(Path.Combine("icons", "folderslist", "outbox.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            if (folder.Type == FolderType.Spam)
            {
                return UIImage.FromBundle(Path.Combine("icons", "folderslist", "spam.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            if (folder.Type == FolderType.Draft)
            {
                return UIImage.FromBundle(Path.Combine("icons", "folderslist", "draft.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            return UIImage.FromBundle(Path.Combine("icons", "folderslist", "folder.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
        }
    }
}
