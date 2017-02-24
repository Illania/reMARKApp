//
// Project: Mark5.Mobile.IOS
// File: CategoriesListViewCell.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class CategoriesTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("CategoriesTableViewCell");
        public static readonly UINib Nib = UINib.FromName("CategoriesTableViewCell", NSBundle.MainBundle);

        public Category Category
        {
            get;
            private set;
        }

        UIColor categoryColor;

        public CategoriesTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static CategoriesTableViewCell Create()
        {
            return (CategoriesTableViewCell)Nib.Instantiate(null, null)[0];
        }

        #region UITableViewCell overrides

        public override void SetSelected(bool selected, bool animated)
        {
            base.SetSelected(selected, animated);

            CategoryColorView.BackgroundColor = categoryColor;
        }

        #endregion

        #region Custom methods

        public void Initialize(Category category)
        {
            Category = category;

            categoryColor = UI.UIColorFromHexString(category.HexColor);
            CategoryColorView.BackgroundColor = categoryColor;
            CategoryNameLabel.Text = category.Name;
        }

        #endregion
    }
}
