//
// Project: Mark5.Mobile.IOS
// File: WaitTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class WaitTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("WaitTableViewCell");
        public static readonly UINib Nib = UINib.FromName("WaitTableViewCell", NSBundle.MainBundle);

        public static WaitTableViewCell Create()
        {
            return (WaitTableViewCell) Nib.Instantiate(null, null)[0];
        }

        protected WaitTableViewCell(IntPtr handle)
            : base(handle)
        {
        }
    }
}