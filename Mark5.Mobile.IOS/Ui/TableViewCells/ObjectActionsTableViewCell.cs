//
// Project: Mark5.Mobile.IOS
// File: ObjectActionsTableViewCell.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class ObjectActionsTableViewCell : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("ObjectActionsTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("ObjectActionsTableViewCell");

        public ObjectActionsTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static ObjectActionsTableViewCell Create()
        {
            var cell = (ObjectActionsTableViewCell) Nib.Instantiate(null, null)[0];

            cell.UsernameLabel.Font = Theme.DefaultBoldFont;
            cell.DateLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            return cell;
        }

        public void Initialize(ObjectAction action)
        {
            UsernameLabel.Text = action.Username ?? action.UserId.ToString();
            DateLabel.Text = action.ActionTimeTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsCompactShortDateTimeString();
            DescriptionLabel.Text = action.Description;
        }
    }
}