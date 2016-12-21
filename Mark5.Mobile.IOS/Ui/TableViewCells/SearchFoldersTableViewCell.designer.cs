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
	[Register ("SearchFoldersTableViewCell")]
	partial class SearchFoldersTableViewCell
	{
		[Outlet]
		UIKit.UIImageView FolderIconImage { get; set; }

		[Outlet]
		UIKit.UILabel FolderNameLabel { get; set; }

		[Outlet]
		UIKit.UILabel FolderPathLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (FolderIconImage != null) {
				FolderIconImage.Dispose ();
				FolderIconImage = null;
			}

			if (FolderNameLabel != null) {
				FolderNameLabel.Dispose ();
				FolderNameLabel = null;
			}

			if (FolderPathLabel != null) {
				FolderPathLabel.Dispose ();
				FolderPathLabel = null;
			}
		}
	}
}
