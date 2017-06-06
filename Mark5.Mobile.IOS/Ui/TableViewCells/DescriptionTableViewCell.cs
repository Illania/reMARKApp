//
// Project: Mark5.Mobile.IOS
// File: DescriptionTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class DescriptionTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("DescriptionTableViewCell");
        public static readonly UINib Nib = UINib.FromName("DescriptionTableViewCell", NSBundle.MainBundle);

        protected DescriptionTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static DescriptionTableViewCell Create()
        {
            var cell = (DescriptionTableViewCell) Nib.Instantiate(null, null)[0];
            cell.DescriptionLabel.Font = Theme.DefaultFont;
            return cell;
        }

        public void Initialize(string description)
        {
            DescriptionLabel.Text = description;
        }
    }
}