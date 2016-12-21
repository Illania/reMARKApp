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
	[Register ("FoldersListViewCell")]
	partial class FoldersListViewCell
	{
		[Outlet]
		UIKit.UIButton ExpandCollapseButton { get; set; }

		[Outlet]
		UIKit.UIImageView FolderCheckedIndicatorImage { get; set; }

		[Outlet]
		UIKit.UIImageView FolderIconImage { get; set; }

		[Outlet]
		UIKit.UILabel FolderNameLabel { get; set; }

		[Outlet]
		UIKit.UIImageView OfflineIndicatorImage { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint OfflineIndicatorLeadingConstraint { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint OfflineIndicatorWidthConstraint { get; set; }

		[Action ("ExpandCollapseButtonTouchUpInside:")]
		partial void ExpandCollapseButtonTouchUpInside (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (ExpandCollapseButton != null) {
				ExpandCollapseButton.Dispose ();
				ExpandCollapseButton = null;
			}

			if (FolderCheckedIndicatorImage != null) {
				FolderCheckedIndicatorImage.Dispose ();
				FolderCheckedIndicatorImage = null;
			}

			if (FolderIconImage != null) {
				FolderIconImage.Dispose ();
				FolderIconImage = null;
			}

			if (FolderNameLabel != null) {
				FolderNameLabel.Dispose ();
				FolderNameLabel = null;
			}

			if (OfflineIndicatorImage != null) {
				OfflineIndicatorImage.Dispose ();
				OfflineIndicatorImage = null;
			}

			if (OfflineIndicatorWidthConstraint != null) {
				OfflineIndicatorWidthConstraint.Dispose ();
				OfflineIndicatorWidthConstraint = null;
			}

			if (OfflineIndicatorLeadingConstraint != null) {
				OfflineIndicatorLeadingConstraint.Dispose ();
				OfflineIndicatorLeadingConstraint = null;
			}
		}
	}
}
