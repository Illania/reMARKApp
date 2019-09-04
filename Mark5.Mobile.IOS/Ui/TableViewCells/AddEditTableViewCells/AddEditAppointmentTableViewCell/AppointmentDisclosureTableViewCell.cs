using UIKit;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
{
    public class AppointmentDisclosureTableViewCell : AddEditTableViewCell
    {
        public static readonly string Key = "AppointmentDisclosureTableViewCell";
        protected UIButton ChevronButton;
        protected UILabel Title;
        protected UILabel Label;
        protected UITextView HiddenTextView;

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
            Title.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

            ContentView.AddSubview(Title);

            Label = new UILabel
            {
                ClipsToBounds = true,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                UserInteractionEnabled = false,
                TextAlignment = UITextAlignment.Justified,
                Lines = 0,
            };
            Label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            Label.SetContentCompressionResistancePriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);
            Label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

            ContentView.AddSubview(Label);

            ChevronButton = GetChevron();
            ChevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            ChevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ChevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            ChevronButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(ChevronButton);

            HiddenTextView = new UITextView { TranslatesAutoresizingMaskIntoConstraints = false };
            HiddenTextView.Hidden = true;
            ContentView.Add(HiddenTextView);

            ContentView.AddConstraints(new[]
            {
                Title.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                Title.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),

                Label.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                Label.LeadingAnchor.ConstraintGreaterThanOrEqualTo(Title.TrailingAnchor, HorizontalMargin),
                Label.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                Label.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor),

                ChevronButton.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                ChevronButton.LeadingAnchor.ConstraintEqualTo(Label.TrailingAnchor, InnerHorizontalMargin),
                ChevronButton.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),

                HiddenTextView.HeightAnchor.ConstraintEqualTo(0),
                HiddenTextView.WidthAnchor.ConstraintEqualTo(0),
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
