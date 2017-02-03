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
    [Register("ObjectActionsTableViewCell")]
    partial class ObjectActionsTableViewCell
    {
        [Outlet]
        UILabel DateLabel { get; set; }

        [Outlet]
        UILabel DescriptionLabel { get; set; }

        [Outlet]
        UILabel UsernameLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (DateLabel != null)
            {
                DateLabel.Dispose();
                DateLabel = null;
            }
            if (DescriptionLabel != null)
            {
                DescriptionLabel.Dispose();
                DescriptionLabel = null;
            }
            if (UsernameLabel != null)
            {
                UsernameLabel.Dispose();
                UsernameLabel = null;
            }
        }
    }
}
