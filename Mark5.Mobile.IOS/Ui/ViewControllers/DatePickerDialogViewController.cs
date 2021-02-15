using System;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DatePickerDialogViewController : AbstractViewController
    {
        protected readonly TaskCompletionSource<DateTime> tcs = new TaskCompletionSource<DateTime>();
        public Task<DateTime> Result => tcs.Task;

        protected UIDatePicker datePicker;
        protected UIView containerView;
        UIButton okButton;
        UIButton cancelButton;
        UIView verticalLine;
        UIView horizontalLine;

        NSLayoutConstraint[] sharedConstraints;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitializeView();
        }

        virtual protected void InitializeView()
        {
            View.BackgroundColor = UIColor.FromWhiteAlpha(0.3f, 0.5f);

            containerView = new UIView
            {
                BackgroundColor = Theme.White,
                LayoutMargins = new UIEdgeInsets(50f, 50f, 50f, 50f),
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            containerView.Layer.CornerRadius = 20;
            containerView.Layer.MasksToBounds = true;

            View.Add(containerView);

            sharedConstraints = new[]
            {
                    containerView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                    containerView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
                    containerView.WidthAnchor.ConstraintEqualTo(Integration.IsIPhone() ? 300f : 350f),
                };

            View.AddConstraints(sharedConstraints);

            datePicker = new UIDatePicker
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Mode = UIDatePickerMode.DateAndTime,
                MinimumDate = NSDate.Now
            };

            cancelButton = new UIButton
            {
                ContentEdgeInsets = new UIEdgeInsets(7f, 7f, 7f, 7f),
                TranslatesAutoresizingMaskIntoConstraints = false,
                Enabled = true
            };
            cancelButton.TitleLabel.Font = Theme.DefaultBoldFont;
            cancelButton.SetTitle(Localization.GetString("cancel"), UIControlState.Normal);
            cancelButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            cancelButton.TitleLabel.Lines = 0;
            cancelButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;

            okButton = new UIButton
            {
                ContentEdgeInsets = new UIEdgeInsets(7f, 7f, 7f, 7f),
                TranslatesAutoresizingMaskIntoConstraints = false,
                Enabled = true
            };
            okButton.TitleLabel.Font = Theme.DefaultFont;
            okButton.SetTitle(Localization.GetString("ok"), UIControlState.Normal);
            okButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            okButton.TitleLabel.Lines = 0;
            okButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;

            horizontalLine = new UIView
            {
                BackgroundColor = Theme.OpaqueLightGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            verticalLine = new UIView
            {
                BackgroundColor = Theme.OpaqueLightGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            containerView.AddSubview(datePicker);
            containerView.AddSubview(horizontalLine);
            containerView.AddSubview(cancelButton);
            containerView.AddSubview(verticalLine);
            containerView.AddSubview(okButton);

            containerView.AddConstraints(new NSLayoutConstraint[]
            {
                    datePicker.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, Integration.IsIPadOrMac() ? 30 : 20),
                    datePicker.LeftAnchor.ConstraintEqualTo(containerView.LeftAnchor, Integration.IsIPadOrMac() ? 30 : 20),
                    datePicker.RightAnchor.ConstraintEqualTo(containerView.RightAnchor, Integration.IsIPadOrMac() ? -30 : -20),
                    datePicker.CenterXAnchor.ConstraintEqualTo(containerView.CenterXAnchor),

                    horizontalLine.TopAnchor.ConstraintEqualTo(datePicker.BottomAnchor),
                    horizontalLine.LeftAnchor.ConstraintEqualTo(containerView.LeftAnchor),
                    horizontalLine.RightAnchor.ConstraintEqualTo(containerView.RightAnchor),
                    horizontalLine.HeightAnchor.ConstraintEqualTo(1f),

                    verticalLine.TopAnchor.ConstraintEqualTo(horizontalLine.BottomAnchor),
                    verticalLine.CenterXAnchor.ConstraintEqualTo(containerView.CenterXAnchor),
                    verticalLine.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor),
                    verticalLine.WidthAnchor.ConstraintEqualTo(1f),

                    cancelButton.TopAnchor.ConstraintEqualTo(horizontalLine.BottomAnchor),
                    cancelButton.LeftAnchor.ConstraintEqualTo(containerView.LeftAnchor),
                    cancelButton.RightAnchor.ConstraintEqualTo(verticalLine.LeftAnchor),

                    okButton.TopAnchor.ConstraintEqualTo(horizontalLine.BottomAnchor),
                    okButton.LeftAnchor.ConstraintEqualTo(verticalLine.RightAnchor),
                    okButton.RightAnchor.ConstraintEqualTo(containerView.RightAnchor),

                    containerView.BottomAnchor.ConstraintEqualTo(cancelButton.BottomAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            okButton.TouchUpInside += OkButton_TouchedUpInside;
            cancelButton.TouchUpInside += CancelButton_TouchedUpInside;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            okButton.TouchUpInside -= OkButton_TouchedUpInside;
            cancelButton.TouchUpInside -= CancelButton_TouchedUpInside;
        }

        protected override void Recycle()
        {
            base.Recycle();

            datePicker = null;
            okButton = null;
            cancelButton = null;
        }

        virtual protected async void OkButton_TouchedUpInside(object sender, EventArgs e)
        {
            containerView.RemoveFromSuperview();

            View.BackgroundColor = Theme.Clear;

            if (datePicker.Date.Compare(NSDate.Now) == NSComparisonResult.Ascending)
            {
                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("selected_time_before_current_time_title"), Localization.GetString("selected_time_before_current_time_content"));
                tcs.SetCanceled();
            }
            else
            {
                var pickedNSDate = datePicker.Date;
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
                    tcs.SetResult(pickedDateTime);
                else
                    tcs.SetCanceled();
            }

            DismissViewController(true, null);
        }

        void CancelButton_TouchedUpInside(object sender, EventArgs e)
        {
            tcs.SetCanceled();
            DismissViewController(true, null);
        }
    }
}