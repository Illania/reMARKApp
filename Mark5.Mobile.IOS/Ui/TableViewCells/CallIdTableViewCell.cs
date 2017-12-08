using System;
using Mark5.Mobile.IOS.Ui.Common;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class CallIdTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(CallIdTableViewCell));

        public bool Toggled { get => settingsSwitch.On; set => settingsSwitch.On = value; }

        public event EventHandler SwitchToggled
        {
            add => settingsSwitch.TouchDown += value;
            remove => settingsSwitch.TouchDown -= value;
        }

        readonly UISwitch settingsSwitch;

        public CallIdTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            settingsSwitch = new UISwitch
            {
                ClipsToBounds = false,
                UserInteractionEnabled = true,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Enabled = true
            };

            AccessoryView = settingsSwitch;
        }
    }
}
