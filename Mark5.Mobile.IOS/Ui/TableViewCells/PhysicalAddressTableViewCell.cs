using System;
using System.IO;
using System.Text;
using Contacts;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class PhysicalAddressTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("PhysicalAddressTableViewCell");
        public static readonly UINib Nib = UINib.FromName("PhysicalAddressTableViewCell", NSBundle.MainBundle);

        public PhysicalAddressTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static PhysicalAddressTableViewCell Create()
        {
            var cell = (PhysicalAddressTableViewCell) Nib.Instantiate(null, null)[0];
            cell.TypeLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-3f);
            cell.AddressLabel.Font = Theme.DefaultFont;
            cell.IconImage.Image = UIImage.FromBundle(Path.Combine("icons", "map.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            return cell;
        }

        public void Initialize(PhysicalAddress pa)
        {
            TypeLabel.Text = Localization.GetString("address").ToUpper();
            if (pa.Type != null && !string.IsNullOrWhiteSpace(pa.Type.Name) && pa.Type.Name != "[None]")
                TypeLabel.Text += " " + pa.Type.Name.ToUpper();

            var cnAddress = new CNMutablePostalAddress
            {
                Street = pa.Street,
                State = pa.Area,
                PostalCode = pa.ZipCode,
                City = pa.City,
                Country = pa.Country.Name,
            };

            var sb = new StringBuilder();
            if (pa.Type != null && pa.Type.Id > 0)
                sb.Append(pa.Type.Name).AppendLine();

            var formatter = new CNPostalAddressFormatter();
            sb.Append(formatter.GetStringFromPostalAddress(cnAddress));
            AddressLabel.Text = sb.ToString();
        }
    }
}