// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    [Register("CommentsTableViewCell")]
    partial class CommentsTableViewCell
    {
        [Outlet]
        UILabel CommentAuthorLabel { get; set; }

        [Outlet]
        UITextView CommentContentLabel { get; set; }

        [Outlet]
        UILabel DateAddedLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (CommentAuthorLabel != null)
            {
                CommentAuthorLabel.Dispose();
                CommentAuthorLabel = null;
            }
            if (CommentContentLabel != null)
            {
                CommentContentLabel.Dispose();
                CommentContentLabel = null;
            }
            if (DateAddedLabel != null)
            {
                DateAddedLabel.Dispose();
                DateAddedLabel = null;
            }
        }
    }
}
