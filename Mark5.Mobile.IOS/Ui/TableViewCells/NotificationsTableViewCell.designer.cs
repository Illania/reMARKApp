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
	[Register ("NotificationsTableViewCell")]
	partial class NotificationsTableViewCell
	{
		[Outlet]
		UIKit.UILabel ContentLabel { get; set; }

		[Outlet]
		UIKit.UILabel DateReceivedLabel { get; set; }

		[Outlet]
		UIKit.UIImageView IconImageView { get; set; }

		[Outlet]
		UIKit.UILabel TitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (IconImageView != null) {
				IconImageView.Dispose ();
				IconImageView = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (ContentLabel != null) {
				ContentLabel.Dispose ();
				ContentLabel = null;
			}

			if (DateReceivedLabel != null) {
				DateReceivedLabel.Dispose ();
				DateReceivedLabel = null;
			}
		}
	}
}
