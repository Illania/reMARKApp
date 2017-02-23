// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    [Register("DocumentsTableViewCell")]
    partial class DocumentsTableViewCell
    {
        [Outlet]
        UIView CategoriesView { get; set; }

        [Outlet]
        UILabel DateReceivedLabel { get; set; }

        [Outlet]
        UIImageView IndicatorImageView1 { get; set; }

        [Outlet]
        UIImageView IndicatorImageView2 { get; set; }

        [Outlet]
        UIImageView IndicatorImageView3 { get; set; }

        [Outlet]
        UIImageView IndicatorImageView4 { get; set; }

        [Outlet]
        UILabel MessagePreviewLabel { get; set; }

        [Outlet]
        UILabel SenderNameLabel { get; set; }

        [Outlet]
        UILabel SubjectLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (CategoriesView != null)
            {
                CategoriesView.Dispose();
                CategoriesView = null;
            }
            if (DateReceivedLabel != null)
            {
                DateReceivedLabel.Dispose();
                DateReceivedLabel = null;
            }
            if (IndicatorImageView1 != null)
            {
                IndicatorImageView1.Dispose();
                IndicatorImageView1 = null;
            }
            if (IndicatorImageView2 != null)
            {
                IndicatorImageView2.Dispose();
                IndicatorImageView2 = null;
            }
            if (IndicatorImageView3 != null)
            {
                IndicatorImageView3.Dispose();
                IndicatorImageView3 = null;
            }
            if (IndicatorImageView4 != null)
            {
                IndicatorImageView3.Dispose();
                IndicatorImageView3 = null;
            }
            if (MessagePreviewLabel != null)
            {
                MessagePreviewLabel.Dispose();
                MessagePreviewLabel = null;
            }
            if (SenderNameLabel != null)
            {
                SenderNameLabel.Dispose();
                SenderNameLabel = null;
            }
            if (SubjectLabel != null)
            {
                SubjectLabel.Dispose();
                SubjectLabel = null;
            }
        }
    }
}
