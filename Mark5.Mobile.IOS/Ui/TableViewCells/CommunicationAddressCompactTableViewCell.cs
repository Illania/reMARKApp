using System;
using System.IO;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class CommunicationAddressCompactTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("CommunicationAddressCompactTableViewCell");

        public static readonly UINib Nib = UINib.FromName("CommunicationAddressCompactTableViewCell", NSBundle.MainBundle);

        public CommunicationAddressCompactTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static CommunicationAddressCompactTableViewCell Create()
        {
            var cell = (CommunicationAddressCompactTableViewCell) Nib.Instantiate(null, null)[0];
            cell.TypeLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-3f);
            cell.AddressLabel.Font = Theme.DefaultFont;
            return cell;
        }

        public void Initialize(CommunicationAddress communicationAddress)
        {
            TypeLabel.Text = GetTypeText(communicationAddress.Type).ToUpper();

            switch (communicationAddress.Type)
            {
                case CommunicationAddressType.Email:
                    AddressLabel.Text = communicationAddress.Address;
                    IconImage.Image = UIImage.FromBundle(Path.Combine("icons", "email.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                case CommunicationAddressType.Mobile:
                    AddressLabel.Text = GetAddressFormatted(communicationAddress);
                    IconImage.Image = UIImage.FromBundle(Path.Combine("icons", "phone.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                case CommunicationAddressType.Phone:
                    AddressLabel.Text = GetAddressFormatted(communicationAddress);
                    IconImage.Image = UIImage.FromBundle(Path.Combine("icons", "phone.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                case CommunicationAddressType.Fax:
                    AddressLabel.Text = GetAddressFormatted(communicationAddress);
                    IconImage.Image = null;
                    break;
                default:
                    AddressLabel.Text = communicationAddress.Address;
                    IconImage.Image = null;
                    break;
            }
        }

        static string GetAddressFormatted(CommunicationAddress ca)
        {
            if (ca.Address.Contains("|"))
                if (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone || ca.Type == CommunicationAddressType.Fax)
                {
                    var addressParts = ca.Address.Split('|');
                    if (addressParts[0].Length > 0)
                        addressParts[0] = "+" + addressParts[0];

                    return string.Join(" ", addressParts.Where(s => !string.IsNullOrWhiteSpace(s)));
                }

            return ca.Address;
        }

        static string GetTypeText(CommunicationAddressType type)
        {
            if (type == CommunicationAddressType.Email)
                return Localization.GetString("email");
            if (type == CommunicationAddressType.Fax)
                return Localization.GetString("fax");
            if (type == CommunicationAddressType.IM)
                return Localization.GetString("im");
            if (type == CommunicationAddressType.Internal)
                return Localization.GetString("internal");
            if (type == CommunicationAddressType.Mobile)
                return Localization.GetString("mobile");
            if (type == CommunicationAddressType.Phone)
                return Localization.GetString("phone");
            if (type == CommunicationAddressType.Skype)
                return Localization.GetString("skype");
            if (type == CommunicationAddressType.System)
                return Localization.GetString("system");
            if (type == CommunicationAddressType.Telex)
                return Localization.GetString("telex");

            return string.Empty;
        }
    }
}