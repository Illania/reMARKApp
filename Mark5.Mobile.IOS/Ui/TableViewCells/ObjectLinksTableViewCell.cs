using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class ObjectLinksTableViewCell : UITableViewCell
    {
        public const float Height = 72f;

        public static readonly UINib Nib = UINib.FromName("ObjectLinksTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("ObjectLinksTableViewCell");

        public ObjectLinksTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static ObjectLinksTableViewCell Create()
        {
            var cell = (ObjectLinksTableViewCell) Nib.Instantiate(null, null)[0];

            cell.TitleLabel.Font = Theme.DefaultBoldFont;

            return cell;
        }

        public void Initialize(ObjectLink link)
        {
            TitleLabel.Text = link.IsReverse ? link.TypeInfo.DescriptionComplexReverse : link.TypeInfo.DescriptionComplex;
            SubtitleLabel.Text = link.Description;
        }
    }
}