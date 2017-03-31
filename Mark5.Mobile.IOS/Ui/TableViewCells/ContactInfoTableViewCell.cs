//
// Project: Mark5.Mobile.IOS
// File: ContactInfoTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    
    public partial class ContactInfoTableViewCell : UITableViewCell
    {
        
        public static readonly NSString Key = new NSString("ContactInfoTableViewCell");
        public static readonly UINib Nib = UINib.FromName("ContactInfoTableViewCell", NSBundle.MainBundle);

        protected ContactInfoTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static ContactInfoTableViewCell Create()
        {
            var cell = (ContactInfoTableViewCell)Nib.Instantiate(null, null)[0];
            cell.TypeLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-3f);
            cell.InfoLabel.Font = Theme.DefaultFont;
            return cell;
        }

        public void Initialize(string type, string info)
        {
            TypeLabel.Text = type;
            InfoLabel.Text = info;
        }

        public void Initialize(string type, NSAttributedString info)
        {
            TypeLabel.Text = type.ToUpper();
            InfoLabel.AttributedText = info;
        }
    }
}
