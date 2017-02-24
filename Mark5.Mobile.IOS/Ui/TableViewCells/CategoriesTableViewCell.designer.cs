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
    [Register("CategoriesTableViewCell")]
    partial class CategoriesTableViewCell
    {
        [Outlet]
        UIView CategoryColorView { get; set; }

        [Outlet]
        UILabel CategoryNameLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (CategoryColorView != null)
            {
                CategoryColorView.Dispose();
                CategoryColorView = null;
            }
            if (CategoryNameLabel != null)
            {
                CategoryNameLabel.Dispose();
                CategoryNameLabel = null;
            }
        }
    }
}
