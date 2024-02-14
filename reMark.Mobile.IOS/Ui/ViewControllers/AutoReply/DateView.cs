using System;
using System.Threading.Tasks;
using AngleSharp.Text;
using CoreGraphics;
using reMark.Mobile.IOS.Model;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;
using System.Globalization;
using Foundation;
using ObjCRuntime;
using static reMark.Mobile.IOS.Model.DateTimeChangeEvent;

namespace reMark.Mobile.IOS.Ui.ViewControllers.AutoReply
{
    public class DateView: AutoReplySubView
    {
        public event EventHandler Edited = delegate { };

        UILabelScalable label;
        public DateRowType RowType;

        UIDatePickerStyled datePicker;
        UIDatePickerStyled timePicker;

        public Action<DateTimeChangeEvent> DateChanged = delegate { };
        public UITextFieldScalable DateTextField;
        public UITextFieldScalable TimeTextField;
        public UILabelScalable Label; 

        public DateView(DateRowType type, Action<DateTimeChangeEvent> dateChanged)
        {
            RowType = type;
            DateChanged += dateChanged;
            Initialize();
        }

        void Initialize()
        {
            label = new UILabelScalable
            {
                Text = RowType == DateRowType.Starts ? Localization.GetString("starts") : Localization.GetString("ends"),
                Font = Theme.DefaultFont.CustomFont(),
                TextColor = Theme.DarkGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
            {
                label.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                label.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin)
            });
            datePicker = new UIDatePickerStyled
            {
                Mode = UIDatePickerMode.Date
            };
            timePicker = new UIDatePickerStyled
            {
                Mode = UIDatePickerMode.Time
            };

            UIToolbar datePickerToolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
            {
                Items = new[]
                {
                    new UIBarButtonItem(UIBarButtonSystemItem.Cancel, this, new Selector("cancelTapped:"))
                    {
                        TintColor = Theme.DarkerBlue
                    },
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem(UIBarButtonSystemItem.Done, this, new Selector("doneTapped:"))
                    {
                        TintColor = Theme.DarkerBlue
                    }
                }
            };

            UIToolbar timePickerToolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
            {
                Items = new[]
             {
                    new UIBarButtonItem(UIBarButtonSystemItem.Cancel, this, new Selector("timePickerCancelTapped:"))
                    {
                        TintColor = Theme.DarkerBlue
                    },
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem(UIBarButtonSystemItem.Done, this, new Selector("timePickerDoneTapped:"))
                    {
                        TintColor = Theme.DarkerBlue
                    }
                }
            };

            DateTextField = new UITextFieldScalable
            {
                Font = Theme.DefaultFont.CustomFont(),
                TintColor = Theme.Clear,
                TextAlignment = UITextAlignment.Right,
                InputView = datePicker,
                InputAccessoryView = datePickerToolbar,
                TranslatesAutoresizingMaskIntoConstraints = false,
                UserInteractionEnabled = true,
                Text = string.Empty
            };

            TimeTextField = new UITextFieldScalable
            {
                Font = Theme.DefaultFont.CustomFont(),
                TintColor = Theme.Clear,
                TextAlignment = UITextAlignment.Right,
                InputView = timePicker,
                InputAccessoryView = timePickerToolbar,
                TranslatesAutoresizingMaskIntoConstraints = false,
                UserInteractionEnabled = true,
                Text = string.Empty
            };

            ContainerView.AddSubview(DateTextField);
            ContainerView.AddSubview(TimeTextField);
            ContainerView.AddConstraints(new[]
            {
                TimeTextField.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                TimeTextField.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin),
                TimeTextField.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                DateTextField.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                DateTextField.RightAnchor.ConstraintEqualTo(TimeTextField.LeftAnchor, -HorizontalMargin),
                DateTextField.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin),
            });

            DateTextField.Started += HandleScrollToView;
            DateTextField.EditingChanged += (sender, e) => Edited(this, EventArgs.Empty);
            TimeTextField.Started += HandleScrollToView;
            TimeTextField.EditingChanged += (sender, e) => Edited(this, EventArgs.Empty);
        }
    

        public override Task InitializeView()
        {

            if (RowType == DateRowType.Starts)
                SetDateAndTime(AutoReplyRule.ActiveFrom.ToLocalTime());
            else
                SetDateAndTime(AutoReplyRule.ActiveTo.ToLocalTime());

            return Task.CompletedTask;
        }

        public override Task UpdateAutoReplyRule()
        {
            if (RowType == DateRowType.Starts)
                InvokeOnMainThread(() => AutoReplyRule.ActiveFrom = (DateTime)datePicker.Date);
            else
                InvokeOnMainThread(() => AutoReplyRule.ActiveTo = (DateTime)datePicker.Date);
            return Task.CompletedTask;
        }

        public void SetDateAndTime(DateTime dateTime)
        {
            UIStringAttributes attributes = new UIStringAttributes(new NSDictionary(
                UIStringAttributeKey.Font, Theme.DefaultFont.CustomFont(),
                UIStringAttributeKey.StrikethroughStyle, NSUnderlineStyle.None
            ));

            NSMutableAttributedString dateString = new NSMutableAttributedString(FormatDateString(dateTime));
            NSMutableAttributedString timeString = new NSMutableAttributedString(FormatTimeString(dateTime));

            dateString.SetAttributes(attributes.Dictionary, new NSRange(0, dateString.Length));
            timeString.SetAttributes(attributes.Dictionary, new NSRange(0, timeString.Length));

            DateTextField.AttributedText = dateString;
            DateTextField.TextColor = UIColor.Black;

            TimeTextField.AttributedText = timeString;
            TimeTextField.TextColor = UIColor.Black;

            SetDateAndTimePicker(dateTime);
        }

        public void SetInvalidDateAndTime(DateTime dateTime)
        {
            var dateString = new NSAttributedString(FormatDateString(dateTime),
                new UIStringAttributes { StrikethroughColor = UIColor.Red, StrikethroughStyle = NSUnderlineStyle.Single });
            var timeString = new NSAttributedString(FormatTimeString(dateTime),
               new UIStringAttributes { StrikethroughColor = UIColor.Red, StrikethroughStyle = NSUnderlineStyle.Single });
            DateTextField.TextColor = UIColor.Red;
            DateTextField.AttributedText = dateString;
            TimeTextField.TextColor = UIColor.Red;
            TimeTextField.AttributedText = timeString;
            SetDateAndTimePicker(dateTime);
        }

        private string FormatDateTimeToString(DateTime dateTime)
        {
            return $"{dateTime.ToString("d MMM yyyy", CultureInfo.CurrentCulture)}{dateTime.ToString("t", CultureInfo.CurrentCulture)}";
        }

        private string FormatDateString(DateTime dateTime)
        {
            return $"{dateTime.Date.ToString("d MMM yyyy", CultureInfo.CurrentCulture)}";
        }

        private string FormatTimeString(DateTime dateTime)
        {
            return $"{dateTime.ToString("t", CultureInfo.CurrentCulture)}";
        }


        private void SetDateAndTimePicker(DateTime dateTime)
        {
            datePicker = new UIDatePickerStyled
            {
                Mode = UIDatePickerMode.Date
            };
            DateTextField.InputView = datePicker;
            SetDatePicker(datePicker, dateTime);

            timePicker = new UIDatePickerStyled
            {
                Mode = UIDatePickerMode.Time
            };
            TimeTextField.InputView = timePicker;
            SetDatePicker(timePicker, dateTime);
        }

        private void SetDatePicker(UIDatePicker picker,DateTime dateTime)
        {
            var fromComponents = new NSDateComponents
            {
                Day = dateTime.Day,
                Month = dateTime.Month,
                Year = dateTime.Year,
                Hour = dateTime.Hour,
                Minute = dateTime.Minute
            };
            var date = new DateTime((int)fromComponents.Year, (int)fromComponents.Month, (int)fromComponents.Day,
                (int)fromComponents.Hour, (int)fromComponents.Minute, 0, DateTimeKind.Local);
            picker.SetDate((NSDate)date, false);
        }

        [Export("doneTapped:")]
        void DoneTapped(UIBarButtonItem sender)
        {
            DateTextField.ResignFirstResponder();
            var selectedDate = datePicker.Date;
            var time = ((DateTime)timePicker.Date).ToLocalTime();
            var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year, selectedDate);
            var dateTime = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, time.Hour, time.Minute, 0, DateTimeKind.Local);
            SetDateAndTime(dateTime);
            DateChanged?.Invoke(new DateTimeChangeEvent(dateTime, RowType));
        }

        [Export("cancelTapped:")]
        void CancelTapped(UIBarButtonItem sender)
        {
            DateTextField.ResignFirstResponder();
        }

        [Export("timePickerDoneTapped:")]
        void TimePickerDoneTapped(UIBarButtonItem sender)
        {
            TimeTextField.ResignFirstResponder();
            var selectedDate = timePicker.Date;
            var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Hour | NSCalendarUnit.Minute, selectedDate);
            var date = (DateTime)datePicker.Date;
            var dateTime = new DateTime(date.Year, date.Month, date.Day,
                (int)selectedDateComponents.Hour, (int)selectedDateComponents.Minute, 0, DateTimeKind.Local);
            SetDateAndTime(dateTime);
            DateChanged?.Invoke(new DateTimeChangeEvent(dateTime, RowType));
        }

        [Export("timePickerCancelTapped:")]
        void TimePickerCancelTapped(UIBarButtonItem sender)
        {
            TimeTextField.ResignFirstResponder();
        }

    }
}

