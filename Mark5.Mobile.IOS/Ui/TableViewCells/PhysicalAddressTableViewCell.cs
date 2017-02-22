//
// Project: Mark5.Mobile.IOS
// File: PhysicalAddressTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using Contacts;
using Foundation;
using Mark5.Mobile.Common.Model;
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
            var cell = (PhysicalAddressTableViewCell)Nib.Instantiate(null, null)[0];
            cell.IconImage.Image = UIImage.FromBundle(Path.Combine("icons", "map.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            return cell;
        }

        public void Initialize(PhysicalAddress pa)
        {
            var cnAddress = new CNMutablePostalAddress
            {
                Street = pa.Street,
                State = pa.Area,
                PostalCode = pa.ZipCode,
                City = pa.City,
                Country = pa.Country.Name
            };

            var formatter = new CNPostalAddressFormatter();
            AddressLabel.Text = formatter.GetStringFromPostalAddress(cnAddress);
        }
    }
}
