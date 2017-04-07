// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    [Register("NotificationsTableViewCell")]
    partial class NotificationsTableViewCell
    {
        [Outlet]
        UIKit.UILabel DateReceivedLabel { get; set; }

        [Outlet]
        UIKit.NSLayoutConstraint FirstLineBottomConstraint { get; set; }

        [Outlet]
        UIKit.NSLayoutConstraint FirstLineHeightConstraint { get; set; }

        [Outlet]
        UIKit.UILabel FirstLineLabel { get; set; }

        [Outlet]
        UIKit.UIImageView IconImageView { get; set; }

        [Outlet]
        UIKit.UIImageView ReadImageView { get; set; }

        [Outlet]
        UIKit.UILabel SecondLineLabel { get; set; }

        [Outlet]
        UIKit.UILabel TitleLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (DateReceivedLabel != null)
            {
                DateReceivedLabel.Dispose();
                DateReceivedLabel = null;
            }

            if (FirstLineLabel != null)
            {
                FirstLineLabel.Dispose();
                FirstLineLabel = null;
            }

            if (IconImageView != null)
            {
                IconImageView.Dispose();
                IconImageView = null;
            }

            if (ReadImageView != null)
            {
                ReadImageView.Dispose();
                ReadImageView = null;
            }

            if (SecondLineLabel != null)
            {
                SecondLineLabel.Dispose();
                SecondLineLabel = null;
            }

            if (TitleLabel != null)
            {
                TitleLabel.Dispose();
                TitleLabel = null;
            }

            if (FirstLineBottomConstraint != null)
            {
                FirstLineBottomConstraint.Dispose();
                FirstLineBottomConstraint = null;
            }

            if (FirstLineHeightConstraint != null)
            {
                FirstLineHeightConstraint.Dispose();
                FirstLineHeightConstraint = null;
            }
        }
    }
}
