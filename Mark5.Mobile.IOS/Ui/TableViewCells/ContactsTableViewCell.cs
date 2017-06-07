using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class ContactsTableViewCell : UITableViewCell
    {
        public const float Height = 50f;

        public static readonly UINib Nib = UINib.FromName("ContactsTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("ContactsTableViewCell");

        UIColor[] categoriesColors;

        public ContactsTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static ContactsTableViewCell Create()
        {
            var cell = (ContactsTableViewCell) Nib.Instantiate(null, null)[0];
            cell.NameLabel.Font = Theme.DefaultFont;
            return cell;
        }

        #region Custom methods

        public void Initialize(ContactPreview contactPreview)
        {
            NameLabel.Text = contactPreview.Name;

            categoriesColors = contactPreview.Categories.Select(c => UI.UIColorFromHexString(c.HexColor)).ToArray();
            UpdateCategoriesColors();
        }

        public void Initialize(Contact contact)
        {
            NameLabel.Text = contact.FullName;
        }

        #endregion

        #region UITableViewCell overrides

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            LeadingConstraint.Constant = Editing ? 0f : -6f;

            Hacks.CorrectFontInActions(this, Theme.DefaultActionsFont);
        }

        public override void SetSelected(bool selected, bool animated)
        {
            base.SetSelected(selected, animated);

            UpdateCategoriesColors();
        }

        public override void SetHighlighted(bool highlighted, bool animated)
        {
            base.SetHighlighted(highlighted, animated);

            UpdateCategoriesColors();
        }

        #endregion

        #region Helper methods

        void UpdateCategoriesColors()
        {
            if (categoriesColors == null)
            {
                foreach (var subView in CategoriesView.Subviews)
                    subView.RemoveFromSuperview();

                return;
            }

            if (CategoriesView.Subviews.Length == categoriesColors.Length)
            {
                for (var i = 0; i < CategoriesView.Subviews.Length; i++)
                    CategoriesView.Subviews[i].BackgroundColor = categoriesColors[i];
            }
            else
            {
                foreach (var subView in CategoriesView.Subviews)
                    subView.RemoveFromSuperview();

                var views = new List<UIView>();
                UIView previousView = null;
                foreach (var color in categoriesColors)
                {
                    var categoryView = new UIView();
                    categoryView.BackgroundColor = color;
                    categoryView.TranslatesAutoresizingMaskIntoConstraints = false;
                    CategoriesView.AddSubview(categoryView);

                    if (previousView == null)
                        CategoriesView.AddConstraint(NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, CategoriesView, NSLayoutAttribute.Top, 1f, 0f));
                    else
                        CategoriesView.AddConstraint(NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, previousView, NSLayoutAttribute.Bottom, 1f, 0f));

                    CategoriesView.AddConstraints(new[]
                    {
                        NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, CategoriesView, NSLayoutAttribute.Left, 1f, 0f),
                        NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, CategoriesView, NSLayoutAttribute.Right, 1f, 0f),
                        NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, 1f)
                    });

                    views.Add(categoryView);
                    previousView = categoryView;
                }

                if (previousView != null)
                    CategoriesView.AddConstraint(NSLayoutConstraint.Create(previousView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, CategoriesView, NSLayoutAttribute.Bottom, 1f, 0f));

                for (var i = 1; i < views.Count; i++)
                    CategoriesView.AddConstraint(NSLayoutConstraint.Create(views[0], NSLayoutAttribute.Height, NSLayoutRelation.Equal, views[i], NSLayoutAttribute.Height, 1f, 0f));
            }
        }

        #endregion
    }
}