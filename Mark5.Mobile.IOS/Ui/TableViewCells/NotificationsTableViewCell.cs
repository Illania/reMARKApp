//
// Project: Mark5.Mobile.IOS
// File: NotificationsTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using System.IO;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class NotificationsTableViewCell : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("NotificationsTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("NotificationsTableViewCell");

        static nfloat firstLineHeightConstraintConstant;
        static nfloat firstLineBottomConstraintConstant;

        public Notification Notification { get; private set; }

        public NotificationsTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static NotificationsTableViewCell Create()
        {
            var cell = (NotificationsTableViewCell) Nib.Instantiate(null, null)[0];

            cell.DateReceivedLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);
            cell.SecondLineLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-1f);
            cell.FirstLineLabel.Font = Theme.DefaultBoldFont;

            firstLineHeightConstraintConstant = cell.FirstLineHeightConstraint.Constant;
            firstLineBottomConstraintConstant = cell.FirstLineBottomConstraint.Constant;

            return cell;
        }

        #region Custom methods

        public void Initialize(Notification notification)
        {
            Notification = notification;

            BackgroundColor = UIColor.White;

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
            IconImageView.Image = icon;

            var splitMessage = notification.Message.Split('\n');

            if (notification.Type == EventType.NewObjectCreated)
            {
                TitleLabel.Font = Theme.DefaultBoldFont;

                TitleLabel.Text = splitMessage.ElementAtOrDefault(0);
                SecondLineLabel.Text = splitMessage.ElementAtOrDefault(1);

                FirstLineLabel.Hidden = true;
                FirstLineHeightConstraint.Constant = 0;
                FirstLineBottomConstraint.Constant = 0;
            }
            else
            {
                TitleLabel.Font = Theme.DefaultFont;

                TitleLabel.Text = notification.Title;
                FirstLineLabel.Text = splitMessage.ElementAtOrDefault(0);
                SecondLineLabel.Text = splitMessage.ElementAtOrDefault(1);

                FirstLineLabel.Hidden = false;
                FirstLineHeightConstraint.Constant = firstLineHeightConstraintConstant;
                FirstLineBottomConstraint.Constant = firstLineBottomConstraintConstant;
            }

            DateReceivedLabel.Text = notification.DateTimeTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsCompactShortDateTimeString();

            ReadImageView.Image = notification.IsRead ? null : UIImage.FromBundle(Path.Combine("icons", "full-dot.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            SelectionStyle = notification.ObjectType == ObjectType.Document ? UITableViewCellSelectionStyle.Default : UITableViewCellSelectionStyle.None;
        }

        #endregion
    }
}