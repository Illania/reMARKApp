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
        public static readonly NSString DefaultId = new NSString("ObjectActionsTableViewCell");

        readonly UITextView descriptionLabel;
        readonly UILabel usernameLabel;
        readonly UILabel dateLabel;

        public ObjectActionsTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            descriptionLabel = new UITextView
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.Black,
                TextAlignment = UITextAlignment.Left,
                Selectable = false,
                Editable = false,
                ScrollEnabled = false,
                ClipsToBounds = false,
                TextContainerInset = UIEdgeInsets.Zero,
                UserInteractionEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            descriptionLabel.TextContainer.LineFragmentPadding = 0f;
            ContentView.Add(descriptionLabel);

            usernameLabel = new UILabel
            {
                Font = Theme.DefaultBoldFont,
                TextColor = Theme.Black,
                TextAlignment = UITextAlignment.Left,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(usernameLabel);

            dateLabel = new UILabel
            {
                Font = Theme.DefaultLightFont.WithRelativeSize(-2f),
                TextColor = Theme.Black,
                TextAlignment = UITextAlignment.Left,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            dateLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            dateLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.Add(dateLabel);

            ContentView.AddConstraints(new[]
            {
                usernameLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                usernameLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),

                dateLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),
                dateLabel.LeadingAnchor.ConstraintEqualTo(usernameLabel.TrailingAnchor, 8f),
                dateLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),

                descriptionLabel.LeadingAnchor.ConstraintEqualTo(usernameLabel.LeadingAnchor),
                descriptionLabel.TrailingAnchor.ConstraintEqualTo(dateLabel.TrailingAnchor),
                descriptionLabel.TopAnchor.ConstraintEqualTo(usernameLabel.BottomAnchor, 4f),
                descriptionLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
            });

        }

        public void Initialize(ObjectAction action)
        {
            usernameLabel.Text = action.Username ?? action.UserId.ToString();
            dateLabel.Text = action.ActionTimeTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime()
                .ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsCompactShortDateTimeString();
            descriptionLabel.Text = action.Description;
        }
    }
}