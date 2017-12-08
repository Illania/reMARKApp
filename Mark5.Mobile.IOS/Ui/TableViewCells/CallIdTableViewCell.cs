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


        readonly UISwitch settingsSwitch;
        readonly UILabel label;
        readonly UIView view;

        public CallIdTableViewCell(Action action)
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            UserInteractionEnabled = true; // if not enabled by default

            view = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.Black
            };

            settingsSwitch = new UISwitch
            {
                UserInteractionEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            label = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.Red,
                Lines = (Int16) 1,
                Text = "Caller identification enabled"
            };

            UITapGestureRecognizer tapRecognizer = new UITapGestureRecognizer(action);
            view.AddGestureRecognizer(tapRecognizer);

            ContentView.AddSubviews(new UIView[] { label, settingsSwitch, view });

            ContentView.AddConstraints(new[]
            {
                label.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                label.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                label.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor),
                label.TrailingAnchor.ConstraintEqualTo(settingsSwitch.LeadingAnchor, -8f),
                settingsSwitch.CenterXAnchor.ConstraintEqualTo(ContentView.CenterXAnchor),
                settingsSwitch.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                view.CenterXAnchor.ConstraintEqualTo(ContentView.CenterXAnchor),
                view.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor)
            });

        }
    }
}
