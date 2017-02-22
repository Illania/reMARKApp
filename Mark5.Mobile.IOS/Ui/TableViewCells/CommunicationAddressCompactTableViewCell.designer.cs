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
    [Register("CommunicationAddressCompactTableViewCell")]
    partial class CommunicationAddressCompactTableViewCell
    {
        [Outlet]
        UIKit.UILabel AddressLabel { get; set; }

        [Outlet]
        UIKit.UIImageView IconImage { get; set; }

        [Action("ActionButtonTouchUpInside:")]
        partial void ActionButtonTouchUpInside(UIKit.UIButton sender);

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
        }
    }
}
