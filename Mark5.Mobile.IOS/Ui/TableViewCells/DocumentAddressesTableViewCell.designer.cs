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
	[Register ("DocumentAddressesTableViewCell")]
	partial class DocumentAddressesTableViewCell
	{
		[Outlet]
		UIKit.UILabel AddressLabel { get; set; }

		[Outlet]
		UIKit.UIImageView ImageIcon { get; set; }

		[Outlet]
		UIKit.UILabel NameLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (AddressLabel != null) {
				AddressLabel.Dispose ();
				AddressLabel = null;
			}

			if (ImageIcon != null) {
				ImageIcon.Dispose ();
				ImageIcon = null;
			}

			if (NameLabel != null) {
				NameLabel.Dispose ();
				NameLabel = null;
			}
		}
	}
}
