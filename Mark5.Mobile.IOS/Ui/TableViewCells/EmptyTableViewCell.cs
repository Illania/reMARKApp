//
// Project: Mark5.Mobile.IOS
// File: EmptyTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    
    public partial class EmptyTableViewCell : UITableViewCell
    {
        
        public static readonly NSString Key = new NSString("EmptyTableViewCell");
        public static readonly UINib Nib = UINib.FromName("EmptyTableViewCell", NSBundle.MainBundle);

        public static EmptyTableViewCell Create()
        {
            return (EmptyTableViewCell)Nib.Instantiate(null, null)[0];
        }

        protected EmptyTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public void Initialize(string text)
        {
            EmptyLabel.Text = text;
        }
    }
}
