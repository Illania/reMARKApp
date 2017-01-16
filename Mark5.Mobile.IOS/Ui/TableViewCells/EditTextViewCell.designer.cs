// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    [Register("EditTextViewCell")]
    partial class EditTextViewCell
    {
        [Outlet]
        UIKit.UITextView ContentTextView { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (ContentTextView != null)
            {
                ContentTextView.Dispose();
                ContentTextView = null;
            }
        }
    }
}
