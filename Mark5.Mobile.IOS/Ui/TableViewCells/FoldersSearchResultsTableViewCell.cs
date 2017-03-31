//
// Project: Mark5.Mobile.IOS
// File: FoldersSearchResultsTableViewCell.cs
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
    
    public partial class FoldersSearchResultsTableViewCell : UITableViewCell
    {

        public const float Height = 65f;

        public static readonly NSString Key = new NSString("FoldersSearchResultsTableViewCell");
        public static readonly UINib Nib = UINib.FromName("FoldersSearchResultsTableViewCell", NSBundle.MainBundle);

        public static FoldersSearchResultsTableViewCell Create()
        {
            return (FoldersSearchResultsTableViewCell)Nib.Instantiate(null, null)[0];
        }

        protected FoldersSearchResultsTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public void Initialize(Folder folder)
        {
            FolderNameLabel.Text = folder.Name;
            FolderPathLabel.Text = folder.Path;
            FolderIconImage.Image = GetIcon(folder);
        }

        public void Disable()
        {
            FolderNameLabel.TextColor = Theme.DarkGray;
            FolderPathLabel.TextColor = Theme.DarkGray;
            FolderIconImage.TintColor = Theme.DarkGray;

            SelectionStyle = UITableViewCellSelectionStyle.None;
            UserInteractionEnabled = false;
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

            if (folder.Type == FolderType.Draft)
            {
                return UIImage.FromBundle(Path.Combine("icons", "folderslist", "draft.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            return UIImage.FromBundle(Path.Combine("icons", "folderslist", "folder.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
        }
    }
}
