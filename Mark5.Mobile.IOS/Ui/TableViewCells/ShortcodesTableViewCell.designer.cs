// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    [Register("ShortcodesTableViewCell")]
    partial class ShortcodesTableViewCell
    {
        [Outlet]
        UIKit.UILabel DescriptionLabel { get; set; }

        [Outlet]
        UIKit.UILabel NameLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (DescriptionLabel != null)
            {
                DescriptionLabel.Dispose();
                DescriptionLabel = null;
            }
            if (NameLabel != null)
            {
                NameLabel.Dispose();
                NameLabel = null;
            }
        }
    }
}
