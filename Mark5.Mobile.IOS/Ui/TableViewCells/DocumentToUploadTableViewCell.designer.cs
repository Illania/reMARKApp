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
    [Register ("DocumentToUploadTableViewCell")]
    partial class DocumentToUploadTableViewCell
    {
        [Outlet]
        UIKit.UIImageView IndicatorImageView1 { get; set; }

        [Outlet]
        UIKit.UILabel SenderLabel { get; set; }

        [Outlet]
        UIKit.UILabel SubjectLabel { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (IndicatorImageView1 != null) {
                IndicatorImageView1.Dispose ();
                IndicatorImageView1 = null;
            }

            if (SenderLabel != null) {
                SenderLabel.Dispose ();
                SenderLabel = null;
            }

            if (SubjectLabel != null) {
                SubjectLabel.Dispose ();
                SubjectLabel = null;
            }
        }
    }
}
