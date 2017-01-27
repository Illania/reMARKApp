// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    [Register("NotificationsTableViewCell")]
    partial class NotificationsTableViewCell
    {
        [Outlet]
        UILabel DateLabel { get; set; }

        [Outlet]
        UIImageView IconView { get; set; }

        [Outlet]
        UILabel MessageLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (DateLabel != null)
            {
                DateLabel.Dispose();
                DateLabel = null;
            }
            if (IconView != null)
            {
                IconView.Dispose();
                IconView = null;
            }
            if (MessageLabel != null)
            {
                MessageLabel.Dispose();
                MessageLabel = null;
            }
        }
    }
}
