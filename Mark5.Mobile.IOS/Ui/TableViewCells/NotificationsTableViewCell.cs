//
// Project: Mark5.Mobile.IOS
// File: NotificationsTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    
    public partial class NotificationsTableViewCell : UITableViewCell
    {
        
        public static readonly UINib Nib = UINib.FromName("NotificationsTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("NotificationsTableViewCell");

        public Notification Notification
        {
            get;
            private set;
        }

        public NotificationsTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static NotificationsTableViewCell Create()
        {
            var cell = (NotificationsTableViewCell)Nib.Instantiate(null, null)[0];

            cell.DateLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2.0f);

            return cell;
        }

        #region Custom methods

        public void Initialize(Notification notification)
        {
            Notification = notification;

            BackgroundColor = notification.IsRead ? UIColor.White : Theme.LightGray;
            MessageLabel.Font = notification.IsRead ? Theme.DefaultFont : Theme.DefaultBoldFont;

            UIImage icon;
            switch (notification.ObjectType)
            {
                case ObjectType.Document:
                    icon = UIImage.FromBundle(Path.Combine("icons", "documents-small.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
                default:
                    icon = UIImage.FromBundle(Path.Combine("icons", "notifications-small.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    break;
            }
            IconView.Image = icon;

            MessageLabel.Text = notification.Message;
            DateLabel.Text = notification.DateTimeTimestamp
                    .ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToServerTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatServerTimestampAsCompactShortDateTimeString();
        }

        #endregion
    }
}
