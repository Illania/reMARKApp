// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    [Register("DocumentAddressesTableViewCell")]
    partial class DocumentAddressesTableViewCell
    {
        [Outlet]
        UIKit.UILabel AttentionLabel { get; set; }

        [Outlet]
        UIKit.UIImageView IconImage { get; set; }

        [Outlet]
        UIKit.UILabel AddressLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (AttentionLabel != null)
            {
                AttentionLabel.Dispose();
                AttentionLabel = null;
            }

            if (IconImage != null)
            {
                IconImage.Dispose();
                IconImage = null;
            }

            if (AddressLabel != null)
            {
                AddressLabel.Dispose();
                AddressLabel = null;
            }
        }
    }
}
