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
using Foundation;
using Mark5.Mobile.Common.Model;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{

    public partial class DocumentsTableViewCell : UITableViewCell
    {

        public static readonly UINib Nib = UINib.FromName("DocumentsTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("DocumentsTableViewCell");

        public DocumentPreview DocumentPreview
        {
            get;
            private set;
        }

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

        public void Initialize(DocumentPreview documentPreview, bool local = false)
        {
            DocumentPreview = documentPreview;

            UpdateCategoriesView();

            SenderNameLabel.Text = !local ? documentPreview.From : documentPreview.To;
            SubjectLabel.Text = documentPreview.Subject;
            MessagePreviewLabel.Text = !string.IsNullOrWhiteSpace(documentPreview.Preview) ? documentPreview.Preview : "This message has no content.";
            DateReceivedLabel.Text = documentPreview.DateReceived.ToCompactDateTimeString();

            UIImage directionIcon = null;
            if (local)
            {
                if (documentPreview.Failed)
                {
                    directionIcon = UIImage.FromBundle(Path.Combine("Icons", "failed.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                }
                else
                {
                    directionIcon = UIImage.FromBundle(Path.Combine("Icons", "pending.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                }
            }
            else if (documentPreview.Direction == Direction.Incoming)
            {
                directionIcon = UIImage.FromBundle(Path.Combine("Icons", "incoming.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }
            else if (documentPreview.Direction == Direction.Outgoing)
            {
                directionIcon = UIImage.FromBundle(Path.Combine("Icons", "outgoing.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }
            if (documentPreview.Direction == Direction.Draft)
            {
                directionIcon = UIImage.FromBundle(Path.Combine("Icons", "pencil.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }


            UpdateReadStatus(local);

            IndicatorImageView1.Image = directionIcon;
            IndicatorImageView3.Image = documentPreview.HasAttachments ? UIImage.FromBundle(Path.Combine("Icons", "attachment.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate) : null;
        }

        #endregion

        #region UITableViewCell overrides

        public override void SetSelected(bool selected, bool animated)
        {
            var colors = new Queue<UIColor>();
            foreach (var view in CategoriesView.Subviews)
            {
                colors.Enqueue(view.BackgroundColor);
            }

            base.SetSelected(selected, animated);

            foreach (var view in CategoriesView.Subviews)
            {
                view.BackgroundColor = colors.Dequeue();
            }
        }

        #endregion

        #region Helper methods

        public void UpdateReadStatus(bool local = false)
        {
            IndicatorImageView2.Image = DocumentPreview.IsRead || local ? null : UIImage.FromBundle(Path.Combine("Icons", "full-dot.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
        }

        public void UpdateCategoriesView()
        {
            foreach (var subView in CategoriesView.Subviews)
            {
                subView.RemoveFromSuperview();
            }

            var views = new List<UIView>();
            UIView previousView = null;
            foreach (var category in DocumentPreview.Categories)
            {
                var categoryView = new UIView();
                categoryView.BackgroundColor = Ui.UiColorFromHexString(category.HexColor);
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
                        NSLayoutConstraint.Create(categoryView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1.0f, 1.0f),
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
