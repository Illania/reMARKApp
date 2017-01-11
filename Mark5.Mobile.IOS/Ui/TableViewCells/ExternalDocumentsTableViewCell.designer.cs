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
    [Register("ExternalDocumentsTableViewCell")]
    partial class ExternalDocumentsTableViewCell
    {
        [Outlet]
        UIView CategoriesView { get; set; }

        [Outlet]
        UILabel DateReceivedLabel { get; set; }

        [Outlet]
        UILabel NameLabel { get; set; }

        [Outlet]
        UILabel PreviewLabel { get; set; }

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
            if (NameLabel != null)
            {
                NameLabel.Dispose();
                NameLabel = null;
            }
            if (PreviewLabel != null)
            {
                PreviewLabel.Dispose();
                PreviewLabel = null;
            }
        }
    }
}
