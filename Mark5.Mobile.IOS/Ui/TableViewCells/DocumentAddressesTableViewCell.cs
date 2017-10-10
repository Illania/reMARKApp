using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class DocumentAddressesTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("DocumentAddressesTableViewCell");
        public static readonly UINib Nib = UINib.FromName("DocumentAddressesTableViewCell", NSBundle.MainBundle);

        protected DocumentAddressesTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static DocumentAddressesTableViewCell Create()
        {
            var cell = (DocumentAddressesTableViewCell)Nib.Instantiate(null, null)[0];
            cell.AttentionLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);
            cell.IconImage.Image = UIImage.FromBundle(Path.Combine("icons", "email.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            return cell;
        }

        public void Initialize(DocumentAddress documentAddress)
        {
            AddressLabel.Text = string.IsNullOrEmpty(documentAddress.Name) ? documentAddress.Address : $"{documentAddress.Name} <{documentAddress.Address}>";
            AttentionLabel.Text = documentAddress.FullAttention;
        }
    }
}