// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    [Register("ShortcodeInfoTableViewCell")]
    partial class ShortcodeInfoTableViewCell
    {
        [Outlet]
        UIKit.UITextView InfoTextView { get; set; }

        [Outlet]
        UIKit.UILabel TypeLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (InfoTextView != null)
            {
                InfoTextView.Dispose();
                InfoTextView = null;
            }

            if (TypeLabel != null)
            {
                TypeLabel.Dispose();
                TypeLabel = null;
            }
        }
    }
}
