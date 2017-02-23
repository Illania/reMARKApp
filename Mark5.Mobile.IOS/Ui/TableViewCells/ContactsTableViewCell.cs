//
// Project: Mark5.Mobile.Common.iOS
// File: ContactsTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    
    public partial class ContactsTableViewCell : UITableViewCell
    {

        public const float Height = 65f;
        
        public static readonly UINib Nib = UINib.FromName("ContactsTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("ContactsTableViewCell");

        public ContactsTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static ContactsTableViewCell Create()
        {
            var cell = (ContactsTableViewCell)Nib.Instantiate(null, null)[0];
            cell.NameLabel.Font = Theme.DefaultBoldFont;
            return cell;
        }

        #region Custom methods

        public void Initialize(ContactPreview contactPreview)
        {
            NameLabel.Text = contactPreview.Name;
            DescriptionLabel.Text = contactPreview.Description;

            UpdateCategoriesView(contactPreview);
        }

        #endregion

        #region UITableViewCell overrides

        public override void SetSelected(bool selected, bool animated)
        {
            var colors = new Queue<UIColor>();
            foreach (var view in CategoriesView.Subviews)
                colors.Enqueue(view.BackgroundColor);

            base.SetSelected(selected, animated);

            foreach (var view in CategoriesView.Subviews)
                view.BackgroundColor = colors.Dequeue();
        }

        #endregion

        #region Helper methods

        public void UpdateCategoriesView(ContactPreview contactPreview)
        {
            foreach (var subView in CategoriesView.Subviews)
            {
                subView.RemoveFromSuperview();
            }

            var views = new List<UIView>();
            UIView previousView = null;
            foreach (var category in contactPreview.Categories)
            {
                var categoryView = new UIView();
                categoryView.BackgroundColor = UI.UIColorFromHexString(category.HexColor);
                categoryView.TranslatesAutoresizingMaskIntoConstraints = false;
                CategoriesView.AddSubview(categoryView);
                if (previousView == null)
                {
                    CategoriesView.AddConstraint(NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, CategoriesView, NSLayoutAttribute.Top, 1.0f, 0.0f));
                }
                else
                {
                    CategoriesView.AddConstraint(NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, previousView, NSLayoutAttribute.Bottom, 1.0f, 0.0f));
                }
                CategoriesView.AddConstraints(new[]
                    {
                        NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, CategoriesView, NSLayoutAttribute.Left, 1.0f, 0.0f),
                        NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, CategoriesView, NSLayoutAttribute.Right, 1.0f, 0.0f),
                        NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1.0f, 1.0f)
                    });

                views.Add(categoryView);
                previousView = categoryView;
            }

            if (previousView != null)
            {
                CategoriesView.AddConstraint(NSLayoutConstraint.Create(previousView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, CategoriesView, NSLayoutAttribute.Bottom, 1.0f, 0.0f));
            }

            for (int i = 1; i < views.Count; i++)
            {
                CategoriesView.AddConstraint(NSLayoutConstraint.Create(views[0], NSLayoutAttribute.Height, NSLayoutRelation.Equal, views[i], NSLayoutAttribute.Height, 1.0f, 0.0f));
            }
        }

        #endregion
        
    }
}
