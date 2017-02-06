//
// Project: Mark5.Mobile.IOS
// File: DocumentAddressesTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    
    public partial class DocumentAddressesTableViewCell : UITableViewCell
    {
    
        public static readonly NSString Key = new NSString("DocumentAddressesTableViewCell");
        public static readonly UINib Nib = UINib.FromName("DocumentAddressesTableViewCell", NSBundle.MainBundle);

        DocumentAddress documentAddress;

        public event EventHandler<DocumentAddress> ActionClicked;

        protected DocumentAddressesTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static DocumentAddressesTableViewCell Create()
        {
            var cell = (DocumentAddressesTableViewCell)Nib.Instantiate(null, null)[0];

            cell.AddressLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2.0f);
            cell.ActionButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "email.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);

            return cell;
        }

        public void Initialize(DocumentAddress documentAddress)
        {
            this.documentAddress = documentAddress;

            NameLabel.Text = documentAddress.Name;
            AddressLabel.Text = documentAddress.FullAddress;
        }

        partial void ActionButtonTouchUpInside(UIButton sender)
        {
            if (ActionClicked != null)
                ActionClicked(this, documentAddress);
        }
    }
}
