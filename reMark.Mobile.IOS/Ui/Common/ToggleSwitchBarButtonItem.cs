
using UIKit;
using CoreGraphics;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Utilities.Extensions;
public class ToggleSwitchBarButtonItem
{
    private UIView? toggleView;
    private bool isOn;

    public event EventHandler<bool>? ToggleSwitchValueChanged;


    public UIBarButtonItem CreateToggleBarButtonItem()
    {

        toggleView = new UIView
        {
            Frame = new CGRect(0, 0, 135, 30) // Set the frame size as needed
        };

        UILabel toggleLabel = new UILabel
        {
            Frame = new CGRect(0, 0, 100, 30),
            Text = "Unread only",
            Font = Theme.DefaultFont.CustomFont().WithRelativeSize(-2f),
            TextColor = Theme.DarkBlue,
            TextAlignment = UITextAlignment.Right
        };

        UISwitch toggleSwitch = new UISwitch
        {
            Frame = new CGRect(100, 0, 35, 30), // Adjust size and position
            Transform = CGAffineTransform.MakeScale(0.7f, 0.7f), // Adjust scale to resize the switch visually
            On = isOn
        };

        toggleSwitch.ValueChanged += (sender, e) =>
        {
            isOn = toggleSwitch.On;
            ToggleSwitchValueChanged?.Invoke(this, isOn);
        };

        toggleView.AddSubviews(toggleLabel, toggleSwitch);

        UITapGestureRecognizer tapGesture = new UITapGestureRecognizer(() =>
        {
            toggleSwitch.On = !toggleSwitch.On;
            isOn = toggleSwitch.On;
            ToggleSwitchValueChanged?.Invoke(this, isOn);
        });
        toggleView.AddGestureRecognizer(tapGesture);

        UIBarButtonItem toggleBarButtonItem = new UIBarButtonItem(toggleView);

        return toggleBarButtonItem;
    }

}