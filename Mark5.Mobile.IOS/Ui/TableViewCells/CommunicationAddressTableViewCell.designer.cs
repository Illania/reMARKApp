// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    [Register("CommunicationAddressTableViewCell")]
    partial class CommunicationAddressTableViewCell
    {
        [Outlet]
        UIKit.UILabel AddressLabel { get; set; }

        [Outlet]
        UIKit.UIImageView IconImage { get; set; }

        [Outlet]
        UIKit.UILabel DescriptionLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (AddressLabel != null)
            {
                AddressLabel.Dispose();
                AddressLabel = null;
            }

            if (IconImage != null)
            {
                IconImage.Dispose();
                IconImage = null;
            }

            if (DescriptionLabel != null)
            {
                DescriptionLabel.Dispose();
                DescriptionLabel = null;
            }
        }
    }
}
