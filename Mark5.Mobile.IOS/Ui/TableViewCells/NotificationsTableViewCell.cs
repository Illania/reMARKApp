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
    public class NotificationsTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(NotificationsTableViewCell));
        public static readonly NSString NewObjectCreatedId = new NSString(nameof(NotificationsTableViewCell) + "_NewObjectCreated");

        readonly UILabel firstLineLabel;
        readonly UILabel secondLineLabel;
        readonly UILabel titleLabel;
        readonly UILabel dateReceivedLabel;
        readonly UIImageView readImageView;
        readonly UIImageView iconImageView;

        public NotificationsTableViewCell(NSString reuseIdentifier)
            : base(UITableViewCellStyle.Default, reuseIdentifier)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            if (reuseIdentifier == DefaultId || reuseIdentifier == NewObjectCreatedId)
            {
                secondLineLabel = new UILabel
                {
                    Font = Theme.DefaultLightFont.WithRelativeSize(-2f),
                    TextColor = Theme.Black,
                    TextAlignment = UITextAlignment.Left,
                    Lines = 1,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                ContentView.Add(secondLineLabel);

                titleLabel = new UILabel
                {
                    Font = Theme.DefaultFont,
                    TextColor = Theme.Black,
                    TextAlignment = UITextAlignment.Left,
                    Lines = 1,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                titleLabel.SetContentHuggingPriority((float)UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Horizontal);
                ContentView.Add(titleLabel);

                dateReceivedLabel = new UILabel
                {
                    Font = Theme.DefaultLightFont.WithRelativeSize(-2f),
                    TextColor = Theme.Black,
                    TextAlignment = UITextAlignment.Left,
                    Lines = 1,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                dateReceivedLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                dateReceivedLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                ContentView.Add(dateReceivedLabel);

                readImageView = new UIImageView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                ContentView.Add(readImageView);

                iconImageView = new UIImageView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                ContentView.Add(iconImageView);

                ContentView.AddConstraints(new[]
                {
                    iconImageView.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),
                    iconImageView.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                    iconImageView.HeightAnchor.ConstraintEqualTo(15f),
                    iconImageView.WidthAnchor.ConstraintEqualTo(15f),

                    titleLabel.LeadingAnchor.ConstraintEqualTo(iconImageView.TrailingAnchor, 8f),
                    titleLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),

                    readImageView.TopAnchor.ConstraintEqualTo(iconImageView.BottomAnchor, 4f),
                    readImageView.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                    readImageView.HeightAnchor.ConstraintEqualTo(15f),
                    readImageView.WidthAnchor.ConstraintEqualTo(15f),

                    secondLineLabel.LeadingAnchor.ConstraintEqualTo(titleLabel.LeadingAnchor),
                    secondLineLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor),

                    dateReceivedLabel.LeadingAnchor.ConstraintEqualTo(titleLabel.TrailingAnchor, 8f),
                    dateReceivedLabel.TrailingAnchor.ConstraintEqualTo(secondLineLabel.TrailingAnchor),
                    dateReceivedLabel.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                    dateReceivedLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor,8f),
                });
            }

            if (reuseIdentifier == DefaultId)
            {
                firstLineLabel = new UILabel
                {
                    Font = Theme.DefaultBoldFont,
                    TextColor = Theme.Black,
                    TextAlignment = UITextAlignment.Left,
                    Lines = 1,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                ContentView.AddSubview(firstLineLabel);

                ContentView.AddConstraints(new[]
                {
                    firstLineLabel.LeadingAnchor.ConstraintEqualTo(titleLabel.LeadingAnchor),
                    firstLineLabel.TrailingAnchor.ConstraintEqualTo(dateReceivedLabel.TrailingAnchor),
                    firstLineLabel.TopAnchor.ConstraintEqualTo(dateReceivedLabel.BottomAnchor, 2f),

                    secondLineLabel.TopAnchor.ConstraintEqualTo(firstLineLabel.BottomAnchor, 2)
                });
            }

            if (reuseIdentifier == NewObjectCreatedId)
            {
                ContentView.AddConstraints(new[]
                {
                    secondLineLabel.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor, 2)
                });
            }
        }

        #region Custom methods

        public void Initialize(Notification notification)
        {
            BackgroundColor = Theme.White;

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

            iconImageView.Image = icon;

            var splitMessage = notification.Message.Split('\n');

            if (ReuseIdentifier == NewObjectCreatedId)
            {
                titleLabel.Font = Theme.DefaultBoldFont;

                titleLabel.Text = splitMessage.ElementAtOrDefault(0);
                secondLineLabel.Text = splitMessage.ElementAtOrDefault(1);
            }
            else if (ReuseIdentifier == DefaultId)
            {
                titleLabel.Font = Theme.DefaultFont;

                titleLabel.Text = notification.Title;
                firstLineLabel.Text = splitMessage.ElementAtOrDefault(0);
                secondLineLabel.Text = splitMessage.ElementAtOrDefault(1);
            }

            dateReceivedLabel.Text = notification.DateTimeTimestamp
                .ConvertTimestampMillisecondsToDateTime()
                .ConvertUtcToUserTime()
                .ConvertDateTimeToTimestampMilliseconds()
                .FormatUserTimestampAsCompactShortDateTimeString();
            readImageView.Image = notification.IsRead ? null : UIImage.FromBundle(Path.Combine("icons", "full-dot.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

            SelectionStyle = notification.ObjectType == ObjectType.Document ? UITableViewCellSelectionStyle.Default : UITableViewCellSelectionStyle.None;
        }

        #endregion
    }
}