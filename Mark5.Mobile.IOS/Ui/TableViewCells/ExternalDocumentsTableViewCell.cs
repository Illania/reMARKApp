using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class ExternalDocumentsTableViewCell : UITableViewCell
    {
        public const float Height = 65f;

        public static readonly UINib Nib = UINib.FromName("ExternalDocumentsTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("ExternalDocumentsTableViewCell");

        public ExternalDocumentsTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static ExternalDocumentsTableViewCell Create()
        {
            var cell = (ExternalDocumentsTableViewCell) Nib.Instantiate(null, null)[0];

            cell.NameLabel.Font = Theme.DefaultBoldFont;
            cell.DateReceivedLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);

            return cell;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            Hacks.CorrectFontInActions(this, Theme.DefaultActionsFont);
        }

        #region Custom methods

        public void Initialize(DocumentPreview documentPreview)
        {
            NameLabel.Text = documentPreview.Subject;
            PreviewLabel.Text = !string.IsNullOrWhiteSpace(documentPreview.Preview) ? Regex.Replace(documentPreview.Preview, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline) : Localization.GetString("no_content");
            DateReceivedLabel.Text = documentPreview.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsCompactShortDateTimeString();

            UpdateCategoriesView(documentPreview);
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

        public void UpdateCategoriesView(DocumentPreview documentPreview)
        {
            foreach (var subView in CategoriesView.Subviews)
                subView.RemoveFromSuperview();

            var views = new List<UIView>();
            UIView previousView = null;
            foreach (var category in documentPreview.Categories)
            {
                var categoryView = new UIView();
                categoryView.BackgroundColor = UI.UIColorFromHexString(category.HexColor);
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

        #endregion
    }
}