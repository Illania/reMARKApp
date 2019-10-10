using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class ParticipantsTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(ParticipantsTableViewCell));

        public int CategoryId { get; private set; }

        UILabel label;
        UIImageView statusImage;

        public ParticipantsTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;
            Accessory = UITableViewCellAccessory.None;

            Opaque = false;
            TranslatesAutoresizingMaskIntoConstraints = false;

            statusImage = new UIImageView
            {
                TintColor = Theme.DarkGray,
                TranslatesAutoresizingMaskIntoConstraints = false,
                UserInteractionEnabled = false,
            };

            ContentView.AddSubview(statusImage);

            label = new UILabel()
            {
                Text = "",
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
            };

            ContentView.AddSubview(label);

            ContentView.AddConstraints(new[]
            {
                    statusImage.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor, 5f),
                    statusImage.WidthAnchor.ConstraintEqualTo(18f),
                    statusImage.HeightAnchor.ConstraintEqualTo(statusImage.WidthAnchor),
                    statusImage.CenterYAnchor.ConstraintEqualTo(label.CenterYAnchor),

                    label.LeadingAnchor.ConstraintEqualTo(statusImage.TrailingAnchor, 20f),
                    label.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 2f),
                    label.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -2f),
                    label.HeightAnchor.ConstraintGreaterThanOrEqualTo(20f)
             });
        }

        public void Initialize(ParticipantsViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.Name) || string.IsNullOrEmpty(viewModel.Email))
                label.Text = viewModel.Name + viewModel.Email;
            else
                label.Text = $"{viewModel.Name} <{viewModel.Email}>";

            if (viewModel.Status == ParticipantStatus.Accepted)
                statusImage.Image = UIImage.FromBundle("Participant-Accepted").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            else if (viewModel.Status == ParticipantStatus.Declined)
                statusImage.Image = UIImage.FromBundle("Participant-Declined").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            else
                statusImage.Image = UIImage.FromBundle("Participant-Unknown").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
        }

    }
}