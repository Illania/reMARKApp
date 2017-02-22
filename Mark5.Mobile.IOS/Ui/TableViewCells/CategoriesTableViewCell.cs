//
// Project: Mark5.Mobile.IOS
// File: CategoriesListViewCell.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;

using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class CategoriesListViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("CategoriesListViewCell");
        public static readonly UINib Nib;

        static CategoriesListViewCell()
        {
            Nib = UINib.FromName("CategoriesListViewCell", NSBundle.MainBundle);
        }

        protected CategoriesListViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }
    }
}
