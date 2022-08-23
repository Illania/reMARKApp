using System;
using Foundation;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class ExtraFieldsTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(ExtraFieldsTableViewCell));

        ExtraField extraField = null;
        readonly UILabelScalable title;
        readonly UISwitch toggleSwitch;

        protected float HorizontalMargin = 8f;

        public ExtraFieldsTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {

            SelectionStyle = UITableViewCellSelectionStyle.None;
            Accessory = UITableViewCellAccessory.None;

            title = new UILabelScalable
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont.CustomFont(),
                Text = string.Empty,
            };
            title.ApplyTheme();
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
                title.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
                toggleSwitch.LeadingAnchor.ConstraintGreaterThanOrEqualTo(title.LeadingAnchor, HorizontalMargin),
                toggleSwitch.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                toggleSwitch.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),

            });
        }

        public void Initialize(ExtraField extraField)
        {
            title.Text = extraField.FieldName;
            toggleSwitch.On = PlatformConfig.Preferences.IsExtraFieldEnabled(extraField.FieldId);
            this.extraField = extraField;
            
        }

        public virtual async void Toggle_ValueChanged(object sender, EventArgs e)
        {
            PlatformConfig.Preferences.SetExtraFieldEnabled(extraField.FieldId, toggleSwitch.On); 
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