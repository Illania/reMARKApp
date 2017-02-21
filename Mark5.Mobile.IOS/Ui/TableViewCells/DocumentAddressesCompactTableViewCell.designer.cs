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
	[Register ("DocumentAddressesCompactTableViewCell")]
	partial class DocumentAddressesCompactTableViewCell
	{
		[Outlet]
		UIKit.UILabel AddressLabel { get; set; }

		[Outlet]
		UIKit.UIImageView IconImage { get; set; }

		[Action ("ActionButtonTouchUpInside:")]
		partial void ActionButtonTouchUpInside (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (AddressLabel != null) {
				AddressLabel.Dispose ();
				AddressLabel = null;
			}

			if (IconImage != null) {
				IconImage.Dispose ();
				IconImage = null;
			}
		}
	}
}
