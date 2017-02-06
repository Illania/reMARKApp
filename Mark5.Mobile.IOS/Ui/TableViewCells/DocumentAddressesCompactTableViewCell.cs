//
// Project: Mark5.Mobile.IOS
// File: DocumentAddressesCompactTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common.Model;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    
    public partial class DocumentAddressesCompactTableViewCell : UITableViewCell
    {
    
        public static readonly NSString Key = new NSString("DocumentAddressesCompactTableViewCell");
        public static readonly UINib Nib = UINib.FromName("DocumentAddressesCompactTableViewCell", NSBundle.MainBundle);

        DocumentAddress documentAddress;

        public event EventHandler<DocumentAddress> ActionClicked;

        protected DocumentAddressesCompactTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static DocumentAddressesCompactTableViewCell Create()
        {
            var cell = (DocumentAddressesCompactTableViewCell)Nib.Instantiate(null, null)[0];

            cell.ActionButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "email.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);

            return cell;
        }

        public void Initialize(DocumentAddress documentAddress)
        {
            this.documentAddress = documentAddress;

            AddressLabel.Text = documentAddress.FullAddress;
        }

        partial void ActionButtonTouchUpInside(UIButton sender)
        {
            if (ActionClicked != null)
                ActionClicked(this, documentAddress);
        }
    }
}
