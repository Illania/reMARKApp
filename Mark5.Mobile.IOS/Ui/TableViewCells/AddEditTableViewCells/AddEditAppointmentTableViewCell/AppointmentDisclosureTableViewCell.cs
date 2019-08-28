using UIKit;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
{
    public class AppointmentDisclosureTableViewCell : AddEditTableViewCell
    {
        public static readonly string Key = "AppointmentDisclosureTableViewCell";
        public UIButton ChevronButton;
        public UILabel Title;
        public UITextView Label;

        public AppointmentDisclosureTableViewCell() : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            Title = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                TextColor = Theme.Black,
            };

            Title.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            Title.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(Title);

            Label = new UITextView
            {
                AutocapitalizationType = UITextAutocapitalizationType.Sentences,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                ScrollEnabled = false,
                ClipsToBounds = true,
                InputAccessoryView = new KeyboardObserverInputAccessoryView(),
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Editable = false,
                UserInteractionEnabled = false,
            };

            ContentView.AddSubview(Label);

            ChevronButton = GetChevron();
            ChevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            ChevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(ChevronButton);

            ContentView.AddConstraints(new[]
            {
                Title.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                Title.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor),
                Title.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),

                Label.HeightAnchor.ConstraintGreaterThanOrEqualTo(20f),
                Label.TopAnchor.ConstraintEqualTo(Title.TopAnchor),
                Label.BottomAnchor.ConstraintEqualTo(Title.BottomAnchor),

                ChevronButton.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                ChevronButton.LeadingAnchor.ConstraintEqualTo(Label.TrailingAnchor, InnerHorizontalMargin),
                ChevronButton.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
            });
        }

        public void SetTitle(string title)
        {
            Title.Text = title;
        }

        public void SetLabel(string label)
        {
            Label.Text = label;
        }

        public override void Reset()
        {
            SetErrorState(false);
            Title.Text = string.Empty;
            Label.Text = string.Empty;
        }
    }
}
