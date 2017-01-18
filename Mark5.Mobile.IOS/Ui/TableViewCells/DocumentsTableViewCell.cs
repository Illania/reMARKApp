//
// Project: Mark5.Mobile.Common.iOS
// File: DocumentsTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2014 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{

    public partial class DocumentsTableViewCell : UITableViewCell
    {

        public static readonly UINib Nib = UINib.FromName("DocumentsTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("DocumentsTableViewCell");

        public DocumentsTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static DocumentsTableViewCell Create()
        {
            var cell = (DocumentsTableViewCell)Nib.Instantiate(null, null)[0];
            cell.SenderNameLabel.Font = Theme.DefaultBoldFont;
            cell.DateReceivedLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2.0f);
            cell.MessagePreviewLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2.0f);
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
            MessagePreviewLabel.Text = !string.IsNullOrWhiteSpace(documentPreview.Preview) ? Regex.Replace(documentPreview.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline) : Localization.GetString("no_content");;
            DateReceivedLabel.Text = documentPreview.DateReceivedTimestamp
                         .ConvertTimestampMillisecondsToDateTime()
                         .ConvertUtcToServerTime()
                         .ConvertDateTimeToTimestampMilliseconds()
                         .FormatServerTimestampAsCompactShortDateTimeString();

            UpdateCategoriesView(documentPreview);

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
            IndicatorImageView3.Image = documentPreview.AttachmentsCount > 0 ? UIImage.FromBundle(Path.Combine("icons", "attachment.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate) : null;
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
            {
                subView.RemoveFromSuperview();
            }

            var views = new List<UIView>();
            UIView previousView = null;
            foreach (var category in documentPreview.Categories)
            {
                var categoryView = new UIView();
                categoryView.BackgroundColor = UI.UIColorFromHexString(category.HexColor);
                categoryView.TranslatesAutoresizingMaskIntoConstraints = false;
                CategoriesView.AddSubview(categoryView);

                if (previousView == null)
                    CategoriesView.AddConstraint(NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, CategoriesView, NSLayoutAttribute.Top, 1.0f, 0.0f));
                else
                    CategoriesView.AddConstraint(NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, previousView, NSLayoutAttribute.Bottom, 1.0f, 0.0f));

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
