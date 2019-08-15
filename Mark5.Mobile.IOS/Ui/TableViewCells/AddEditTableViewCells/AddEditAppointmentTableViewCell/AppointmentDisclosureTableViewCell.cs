using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
{
    public class AppointmentDisclosureTableViewCell : AddEditTableViewCell
    {
        public static readonly string Key = "AppointmentDisclosureTableViewCell";
        public UIButton ChevronButton;
        public UILabel Title;
        public UILabel Label;

        public AppointmentDisclosureTableViewCell() : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            Title = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                TextColor = Theme.Black
            };

            Title.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            Title.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(Title);

            Label = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Lines = 0,
            };

            ContentView.AddSubview(Label);

            ChevronButton = GetChevron();
            ChevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
            ChevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContentView.AddSubview(ChevronButton);

            ContentView.AddConstraints(new[]
            {
                Title.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, VerticalMargin),
                Title.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -VerticalMargin),
                Title.LeadingAnchor.ConstraintEqualTo(ContentView.LeadingAnchor),

                Label.HeightAnchor.ConstraintGreaterThanOrEqualTo(20f),
                Label.TopAnchor.ConstraintEqualTo(Title.TopAnchor),
                Label.BottomAnchor.ConstraintEqualTo(Title.BottomAnchor),
                Label.TrailingAnchor.ConstraintEqualTo(ChevronButton.LeadingAnchor, InnerHorizontalMargin),

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
