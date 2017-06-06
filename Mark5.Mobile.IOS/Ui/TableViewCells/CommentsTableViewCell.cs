//
// Project: Mark5.Mobile.IOS
// File: CommentsTableViewCell.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using System.Globalization;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class CommentsTableViewCell : UITableViewCell
    {
        public const float Height = 68f;

        public static readonly NSString Key = new NSString("CommentsTableViewCell");
        public static readonly UINib Nib = UINib.FromName("CommentsTableViewCell", NSBundle.MainBundle);

        public Comment Comment { get; private set; }

        protected CommentsTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static CommentsTableViewCell Create()
        {
            var cell = (CommentsTableViewCell) Nib.Instantiate(null, null)[0];

            cell.CommentAuthorLabel.Font = Theme.DefaultBoldFont;
            cell.DateAddedLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            return cell;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            Hacks.CorrectFontInActions(this, Theme.DefaultActionsFont);
        }

        public void Initialize(Comment comment)
        {
            Comment = comment;

            CommentAuthorLabel.Text = comment.UserId == ServerConfig.SystemSettings.UserInfo.User.Id ? Localization.GetString("me") : comment.UserName.ToUpper(CultureInfo.CurrentCulture);
            DateAddedLabel.Text = comment.DateAddedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsCompactLongDateTimeString();
            CommentContentLabel.Text = comment.Content;
        }
    }
}