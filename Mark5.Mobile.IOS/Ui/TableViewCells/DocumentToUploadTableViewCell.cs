using System;
using System.IO;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class DocumentToUploadTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("DocumentToUploadTableViewCell");
        public static readonly UINib Nib = UINib.FromName("DocumentToUploadTableViewCell", NSBundle.MainBundle);

        public DocumentToUploadTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static DocumentToUploadTableViewCell Create()
        {
            var cell = (DocumentToUploadTableViewCell)Nib.Instantiate(null, null)[0];
            cell.SenderLabel.Font = Theme.DefaultBoldFont;
            return cell;
        }

        public void Initialize(DocumentPreview documentPreview, int section)
        {
            if (section == 0)
            {
                SelectionStyle = UITableViewCellSelectionStyle.None;
                Accessory = UITableViewCellAccessory.None;
                IndicatorImageView1.Image = UIImage.FromBundle(Path.Combine("icons", "pending.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }
            else if (section == 1)
            {
                SelectionStyle = UITableViewCellSelectionStyle.Default;
                Accessory = UITableViewCellAccessory.DisclosureIndicator;
                IndicatorImageView1.Image = UIImage.FromBundle(Path.Combine("icons", "failed.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }
            else
            {
                SelectionStyle = UITableViewCellSelectionStyle.None;
                Accessory = UITableViewCellAccessory.None;
                IndicatorImageView1.Image = null;
            }

            var address = documentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType).FirstOrDefault();
            SenderLabel.Text = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
            SubjectLabel.Text = string.IsNullOrWhiteSpace(documentPreview.Subject) ? Localization.GetString("no_subject") : documentPreview.Subject;

        }
    }
}
