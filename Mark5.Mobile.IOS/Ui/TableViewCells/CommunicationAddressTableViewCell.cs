//
// Project: Mark5.Mobile.IOS
// File: CommunicationAddressTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{

    public partial class CommunicationAddressTableViewCell : UITableViewCell
    {

        public static readonly NSString Key = new NSString("CommunicationAddressTableViewCell");
        public static readonly UINib Nib = UINib.FromName("CommunicationAddressTableViewCell", NSBundle.MainBundle);

        public CommunicationAddressTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static CommunicationAddressTableViewCell Create()
        {
            var cell = (CommunicationAddressTableViewCell)Nib.Instantiate(null, null)[0];
            cell.AddressLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);
            return cell;
        }

        public void Initialize(CommunicationAddress communicationAddress)
        {
            NameLabel.Font = communicationAddress.IsPrimary ? Theme.DefaultBoldFont : Theme.DefaultFont;
            NameLabel.Text = communicationAddress.Description;

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

        string GetAddressFormatted(CommunicationAddress ca)
        {
            if (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone || ca.Type == CommunicationAddressType.Fax)
            {
                var addressParts = ca.Address.Split('|');
                if (addressParts[0].Length > 0)
                {
                    addressParts[0] = "+" + addressParts[0];
                }

                return string.Join(" ", addressParts.Where(s => !string.IsNullOrWhiteSpace(s)));
            }

            return ca.Address;
        }
    }
}
