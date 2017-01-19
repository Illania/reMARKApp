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
    [Register("ContactsTableViewCell")]
    partial class ContactsTableViewCell
    {
        [Outlet]
        UIView CategoriesView { get; set; }

        [Outlet]
        UILabel DescriptionLabel { get; set; }

        [Outlet]
        UILabel NameLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (CategoriesView != null)
            {
                CategoriesView.Dispose();
                CategoriesView = null;
            }
            if (DescriptionLabel != null)
            {
                DescriptionLabel.Dispose();
                DescriptionLabel = null;
            }
            if (NameLabel != null)
            {
                NameLabel.Dispose();
                NameLabel = null;
            }
        }
    }
}
