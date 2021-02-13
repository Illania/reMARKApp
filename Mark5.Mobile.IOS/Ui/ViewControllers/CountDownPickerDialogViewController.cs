using System;
using Foundation;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class CountDownPickerDialogViewController : DatePickerDialogViewController
    {
        protected override void InitializeView()
        {
            base.InitializeView();

            datePicker.Mode = UIDatePickerMode.CountDownTimer;
            datePicker.MinuteInterval = 5;   
            datePicker.MinimumDate = (NSDate)DateTime.SpecifyKind(DateTime.Now.Date.AddHours(-12), DateTimeKind.Utc);
            if (PlatformConfig.Preferences.RememberLastUserDelaySettings == true)
                datePicker.CountDownDuration = PlatformConfig.Preferences.LastUserSendingDelay;
            else
                datePicker.CountDownDuration = 300;
        }

        protected override async void OkButton_TouchedUpInside(object sender, EventArgs e)
        {
            containerView.RemoveFromSuperview();

            View.BackgroundColor = Theme.Clear;

            var pickedNSDate = NSDate.Now.AddSeconds(datePicker.CountDownDuration);
            var pickedDateTime = (DateTime)pickedNSDate;

            var dateFormatter = new NSDateFormatter
            {
                TimeStyle = NSDateFormatterStyle.Short,
                DateStyle = NSDateFormatterStyle.Short,
                Locale = NSLocale.CurrentLocale
            };

            var sendConfirmed = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("confirm_delayed_send_title"),
                                                                      String.Format(Localization.GetString("confirm_delayed_send_content"), dateFormatter.ToString(pickedNSDate)));

            if (sendConfirmed)
            {
                if (PlatformConfig.Preferences.RememberLastUserDelaySettings == true)
                    PlatformConfig.Preferences.LastUserSendingDelay = (int)datePicker.CountDownDuration;
                tcs.SetResult(pickedDateTime);
            }
            else
                tcs.SetCanceled();

            DismissViewController(true, null);
        }
    }
}