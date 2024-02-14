using System;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells
{
    public class ToggleTableViewCell : AddEditTableViewCell
    {
        readonly UILabelScalable title;
        readonly UISwitch toggleSwitch;

        public ToggleTableViewCell(UITableViewCellStyle style, string reuseIdentifier) : base(style, reuseIdentifier)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            title = new UILabelScalable
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont.CustomFont(),
                Text = string.Empty,
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
