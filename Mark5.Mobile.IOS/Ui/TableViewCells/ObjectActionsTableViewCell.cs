using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class ObjectActionsTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(ObjectActionsTableViewCell));

        readonly UILabel usernameLabel;
        readonly UILabel dateLabel;
        readonly UITextView descriptionTextView;

        public ObjectActionsTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;
            Accessory = UITableViewCellAccessory.None;

            usernameLabel = new UILabel
            {
                Font = Theme.DefaultBoldFont,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            dateLabel = new UILabel
            {
                Font = Theme.DefaultLightFont.WithRelativeSize(-2f),
                TextColor = Theme.DarkGray,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            dateLabel.SetContentCompressionResistancePriority(1000f, UILayoutConstraintAxis.Horizontal);
            dateLabel.SetContentHuggingPriority(1000f, UILayoutConstraintAxis.Horizontal);

            descriptionTextView = new UITextView
            {
                Selectable = false,
                Editable = false,
                ScrollEnabled = false,
                ClipsToBounds = false,
                TextContainerInset = UIEdgeInsets.Zero,
                UserInteractionEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            descriptionTextView.ApplyTheme();
            descriptionTextView.TextContainer.LineFragmentPadding = 0f;

            ContentView.Add(usernameLabel);
            ContentView.Add(dateLabel);
            ContentView.Add(descriptionTextView);

            ContentView.AddConstraints(new[]
            {
                usernameLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                usernameLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),

                dateLabel.LeadingAnchor.ConstraintEqualTo(usernameLabel.TrailingAnchor, 8f),
                dateLabel.CenterYAnchor.ConstraintEqualTo(usernameLabel.CenterYAnchor),
                dateLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),

                descriptionTextView.LeadingAnchor.ConstraintEqualTo(usernameLabel.LeadingAnchor),
                descriptionTextView.TrailingAnchor.ConstraintEqualTo(dateLabel.TrailingAnchor),
                descriptionTextView.TopAnchor.ConstraintEqualTo(usernameLabel.BottomAnchor, 4f),
                descriptionTextView.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
            });
        }

        public void Initialize(ObjectAction action)
        {
            usernameLabel.Text = action.Description;
            dateLabel.Text = action.ActionTimeTimestamp
                .ConvertTimestampMillisecondsToDateTime()
                .ConvertUtcToUserTime()
                .ConvertDateTimeToTimestampMilliseconds()
                .FormatUserTimestampAsCompactShortDateTimeString();
            descriptionTextView.Text = action.ActionType;
        }
    }
}