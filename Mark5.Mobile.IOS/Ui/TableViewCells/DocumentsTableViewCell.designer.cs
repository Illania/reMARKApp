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
	[Register ("DocumentsTableViewCell")]
	partial class DocumentsTableViewCell
	{
		[Outlet]
		UIKit.UIView CategoriesView { get; set; }

		[Outlet]
		UIKit.UILabel DateReceivedLabel { get; set; }

		[Outlet]
		UIKit.UIImageView IndicatorImageView1 { get; set; }

		[Outlet]
		UIKit.UIImageView IndicatorImageView2 { get; set; }

		[Outlet]
		UIKit.UIImageView IndicatorImageView3 { get; set; }

		[Outlet]
		UIKit.UIImageView IndicatorImageView4 { get; set; }

		[Outlet]
		UIKit.UILabel MessagePreviewLabel { get; set; }

		[Outlet]
		UIKit.UILabel SenderNameLabel { get; set; }

		[Outlet]
		UIKit.UILabel SubjectLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CategoriesView != null) {
				CategoriesView.Dispose ();
				CategoriesView = null;
			}

			if (DateReceivedLabel != null) {
				DateReceivedLabel.Dispose ();
				DateReceivedLabel = null;
			}

			if (IndicatorImageView1 != null) {
				IndicatorImageView1.Dispose ();
				IndicatorImageView1 = null;
			}

			if (IndicatorImageView2 != null) {
				IndicatorImageView2.Dispose ();
				IndicatorImageView2 = null;
			}

			if (IndicatorImageView3 != null) {
				IndicatorImageView3.Dispose ();
				IndicatorImageView3 = null;
			}

			if (IndicatorImageView4 != null) {
				IndicatorImageView4.Dispose ();
				IndicatorImageView4 = null;
			}

			if (MessagePreviewLabel != null) {
				MessagePreviewLabel.Dispose ();
				MessagePreviewLabel = null;
			}

			if (SenderNameLabel != null) {
				SenderNameLabel.Dispose ();
				SenderNameLabel = null;
			}

			if (SubjectLabel != null) {
				SubjectLabel.Dispose ();
				SubjectLabel = null;
			}
		}
	}
}
