//
// Project: Mark5.Mobile.Common.iOS
// File: DocumentsTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{

    public partial class DocumentsCompactTableViewCell : UITableViewCell
    {

        public const float Height = 68f;

        public static readonly UINib Nib = UINib.FromName("DocumentsCompactTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("DocumentsCompactTableViewCell");

        UIColor[] categoriesColors;

        public DocumentsCompactTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static DocumentsCompactTableViewCell Create()
        {
            var cell = (DocumentsCompactTableViewCell)Nib.Instantiate(null, null)[0];

            cell.SenderNameLabel.Font = Theme.DefaultBoldFont;
            cell.DateReceivedLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);

            return cell;
        }

        #region Custom methods

        public void Initialize(DocumentPreview documentPreview)
        {
            if (documentPreview.Direction == DocumentDirection.Incoming)
            {
                var address = documentPreview.Addresses.FirstOrDefault(da => da.AddressType == DocumentAddressType.From);
                SenderNameLabel.Text = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
            }
            else
            {
                var address = documentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType).FirstOrDefault();
                SenderNameLabel.Text = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
            }

            SubjectLabel.Text = documentPreview.Subject;
            DateReceivedLabel.Text = documentPreview.DateReceivedTimestamp
                         .ConvertTimestampMillisecondsToDateTime()
                         .ConvertUtcToServerTime()
                         .ConvertDateTimeToTimestampMilliseconds()
                         .FormatServerTimestampAsCompactShortDateTimeString();

            categoriesColors = documentPreview.Categories.Select(c => UI.UIColorFromHexString(c.HexColor)).ToArray();
            UpdateCategoriesColors();

            UIImage directionIcon;
            switch (documentPreview.Direction)
            {
                case DocumentDirection.Incoming:
                    directionIcon = UIImage.FromBundle(Path.Combine("icons", "incoming.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                case DocumentDirection.Outgoing:
                    directionIcon = UIImage.FromBundle(Path.Combine("icons", "outgoing.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                case DocumentDirection.Draft:
                    directionIcon = UIImage.FromBundle(Path.Combine("icons", "pencil.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                default:
                    directionIcon = null;
                    break;
            }

            IndicatorImageView1.Image = directionIcon;
            IndicatorImageView2.Image = (PlatformConfig.Preferences.UnreadIndicatorMe ? documentPreview.IsReadByCurrent : documentPreview.IsReadByAnyone) ? null : UIImage.FromBundle(Path.Combine("icons", "full-dot.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
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
                for (int i = 0; i < CategoriesView.Subviews.Length; i++)
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

                for (int i = 1; i < views.Count; i++)
                    CategoriesView.AddConstraint(NSLayoutConstraint.Create(views[0], NSLayoutAttribute.Height, NSLayoutRelation.Equal, views[i], NSLayoutAttribute.Height, 1f, 0f));
            }
        }

        #endregion

    }
}
