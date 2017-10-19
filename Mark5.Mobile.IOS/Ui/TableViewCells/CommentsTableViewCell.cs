using System.Globalization;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class CommentsTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(CommentsTableViewCell));

        readonly UILabel authorLabel;
        readonly UILabel dateLabel;
        readonly UITextView commentTextView;

        public CommentsTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;
            Accessory = UITableViewCellAccessory.None;

            authorLabel = new UILabel
            {
                Font = Theme.DefaultBoldFont,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            dateLabel = new UILabel
            {
                Font = Theme.DefaultFont.WithRelativeSize(-2f),
                TextColor = Theme.DarkGray,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            dateLabel.SetContentHuggingPriority(1000f, UILayoutConstraintAxis.Horizontal);
            dateLabel.SetContentCompressionResistancePriority(1000f, UILayoutConstraintAxis.Horizontal);

            commentTextView = new UITextView
            {
                Selectable = false,
                Editable = false,
                ScrollEnabled = false,
                ClipsToBounds = false,
                TextContainerInset = UIEdgeInsets.Zero,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            commentTextView.ApplyTheme();
            commentTextView.TextContainer.LineFragmentPadding = 0f;

            ContentView.AddSubview(authorLabel);
            ContentView.AddSubview(dateLabel);
            ContentView.AddSubview(commentTextView);

            ContentView.AddConstraints(new[]
            {
                authorLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                authorLabel.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 8f),

                dateLabel.LeadingAnchor.ConstraintEqualTo(authorLabel.TrailingAnchor, 8f),
                dateLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                dateLabel.CenterYAnchor.ConstraintEqualTo(authorLabel.CenterYAnchor),

                commentTextView.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                commentTextView.TopAnchor.ConstraintEqualTo(authorLabel.BottomAnchor, 4f),
                commentTextView.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                commentTextView.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -8f)
            });

        }

        public void Initialize(Comment comment)
        {
            authorLabel.Text = comment.UserId == ServerConfig.SystemSettings.UserInfo.User.Id ? Localization.GetString("me") : comment.UserName.ToUpper(CultureInfo.CurrentCulture);
            dateLabel.Text = comment.DateAddedTimestamp
                .ConvertTimestampMillisecondsToDateTime()
                .ConvertUtcToUserTime()
                .ConvertDateTimeToTimestampMilliseconds()
                .FormatUserTimestampAsCompactLongDateTimeString();
            commentTextView.Text = comment.Content;
        }
    }
}