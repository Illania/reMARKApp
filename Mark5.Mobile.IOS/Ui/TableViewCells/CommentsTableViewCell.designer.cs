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
	[Register ("CommentsTableViewCell")]
	partial class CommentsTableViewCell
	{
		[Outlet]
		UIKit.UILabel CommentAuthorLabel { get; set; }

		[Outlet]
		UIKit.UILabel CommentContentLabel { get; set; }

		[Outlet]
		UIKit.UILabel DateAddedLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CommentAuthorLabel != null) {
				CommentAuthorLabel.Dispose ();
				CommentAuthorLabel = null;
			}

			if (CommentContentLabel != null) {
				CommentContentLabel.Dispose ();
				CommentContentLabel = null;
			}

			if (DateAddedLabel != null) {
				DateAddedLabel.Dispose ();
				DateAddedLabel = null;
			}
		}
	}
}
