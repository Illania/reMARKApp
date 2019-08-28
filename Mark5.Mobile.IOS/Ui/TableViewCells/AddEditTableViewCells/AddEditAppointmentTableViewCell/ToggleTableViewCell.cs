using System;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
{
    public class ToggleTableViewCell : AddEditTableViewCell
    {
        readonly UILabel title;
        readonly UISwitch toggleSwitch;

        public ToggleTableViewCell(UITableViewCellStyle style, string reuseIdentifier) : base(style, reuseIdentifier)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            title = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                Text = string.Empty
            };

            ContentView.Add(title);

            toggleSwitch = new UISwitch
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            toggleSwitch.SetContentHuggingPriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);
            ContentView.Add(toggleSwitch);
            toggleSwitch.ValueChanged += Toggle_ValueChanged;

            ContentView.AddConstraints(new[]
            {
                title.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                title.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor),
                title.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                toggleSwitch.LeadingAnchor.ConstraintGreaterThanOrEqualTo(title.LeadingAnchor, HorizontalMargin),
                toggleSwitch.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                toggleSwitch.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
            });
        }

        public virtual void Toggle_ValueChanged(object sender, EventArgs e)
        {
        }

        public void SetTitle(string title)
        {
            this.title.Text = title;
        }

        public void SetToggleState(bool state)
        {
            toggleSwitch.On = state;
        }
    }
}
