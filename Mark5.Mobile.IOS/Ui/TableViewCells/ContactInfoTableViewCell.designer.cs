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
	[Register ("ContactInfoTableViewCell")]
	partial class ContactInfoTableViewCell
	{
		[Outlet]
		UIKit.UILabel InfoLabel { get; set; }

		[Outlet]
		UIKit.UILabel TypeLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TypeLabel != null) {
				TypeLabel.Dispose ();
				TypeLabel = null;
			}

			if (InfoLabel != null) {
				InfoLabel.Dispose ();
				InfoLabel = null;
			}
		}
	}
}
