//
// Project: ${Project}
// File: CommentsTableViewCell.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Globalization;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class CommentsTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("CommentsTableViewCell");
        public static readonly UINib Nib;

        public Comment Comment
        {
            get;
            private set;
        }

        static CommentsTableViewCell()
        {
            Nib = UINib.FromName("CommentsTableViewCell", NSBundle.MainBundle);
        }

        protected CommentsTableViewCell(IntPtr handle) : base(handle)
        {
        }

        public static CommentsTableViewCell Create()
        {
            var cell = (CommentsTableViewCell)Nib.Instantiate(null, null)[0];

            cell.CommentAuthorLabel.Font = Theme.DefaultBoldFont;
            cell.DateAddedLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2.0f);

            cell.CommentContentLabel.TextContainer.LineFragmentPadding = 0.0f;
            cell.CommentContentLabel.TextContainerInset = UIEdgeInsets.Zero;

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            return cell;
        }

        public void Initialize(Comment comment)
        {
            Comment = comment;

            var username = comment.UserName.Equals(ServerConfig.SystemSettings.UserInfo.User.Username, StringComparison.CurrentCultureIgnoreCase) ?
                                  Localization.GetString("me") : comment.UserName.ToUpper(CultureInfo.CurrentCulture);
            CommentAuthorLabel.Text = username;
            DateAddedLabel.Text = comment.DateAddedTimestamp.ConvertTimestampMillisecondsToDateTime()
                        .ConvertUtcToServerTime()
                        .ConvertDateTimeToTimestampMilliseconds()
                        .FormatServerTimestampAsCompactLongDateTimeString();
            CommentContentLabel.Text = comment.Content;
        }
    }
}
