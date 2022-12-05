using System;
using System.Threading.Tasks;
using AngleSharp.Text;
using CoreGraphics;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;
using System.Globalization;
using Foundation;
using ObjCRuntime;
using static Mark5.Mobile.IOS.Model.DateTimeChangeEvent;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.AutoReply
{
    public class DateView: AutoReplySubView
    {
        public event EventHandler Edited = delegate { };

        UILabelScalable label;
        public DateRowType RowType;

        UIDatePickerStyled datePicker;

        public Action<DateTimeChangeEvent> DateChanged = delegate { };
        public UITextFieldScalable DateTextField;
        public UILabelScalable Label; 


        public DateTime Date { get => (DateTime)datePicker.Date; set => datePicker.Date = (NSDate)value; }

        public bool Empty => string.IsNullOrEmpty(DateTextField?.Text);

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
            }); datePicker = new UIDatePickerStyled
            {
                Mode = UIDatePickerMode.DateAndTime
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

            ContainerView.AddSubview(DateTextField);
            DateTextField.Started += HandleScrollToView;
            DateTextField.EditingChanged += (sender, e) => Edited(this, EventArgs.Empty);
            ContainerView.AddConstraints(new[]
            {
                DateTextField.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                DateTextField.LeftAnchor.ConstraintEqualTo(label.RightAnchor, InnerMargin),
                DateTextField.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                DateTextField.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin)
            });

            DateTextField.Started += HandleScrollToView;
            DateTextField.EditingChanged += (sender, e) => Edited(this, EventArgs.Empty);
        }
    

        public override Task InitializeView()
        {

            if (RowType == DateRowType.Starts)
                SetDateAndTime(AutoReplyRule.ActiveFrom);
            else
                SetDateAndTime(AutoReplyRule.ActiveTo);

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

            NSMutableAttributedString prettyString = new NSMutableAttributedString(FormatDateTimeToString(dateTime));

            prettyString.SetAttributes(attributes.Dictionary, new NSRange(0, prettyString.Length));

            DateTextField.AttributedText = prettyString;
            DateTextField.TextColor = UIColor.Black;

            SetDateAndTimePicker(dateTime);
        }

        public void SetInvalidDateAndTime(DateTime dateTime)
        {
            var attString = new NSAttributedString(FormatDateTimeToString(dateTime), new UIStringAttributes { StrikethroughColor = UIColor.Red, StrikethroughStyle = NSUnderlineStyle.Single });
            DateTextField.TextColor = UIColor.Red;
            DateTextField.AttributedText = attString;
            SetDateAndTimePicker(dateTime);
        }

        public void SetInvalidDate(DateTime dateTime)
        {
            var attString = new NSAttributedString(FormatDateString(dateTime), new UIStringAttributes { StrikethroughColor = UIColor.Red, StrikethroughStyle = NSUnderlineStyle.Single });
            DateTextField.TextColor = UIColor.Red;
            DateTextField.AttributedText = attString;
            SetDateOnlyPicker(dateTime);
        }

        private string FormatDateTimeToString(DateTime dateTime)
        {
            return $"{dateTime.ToString("d MMM yyyy", CultureInfo.CurrentCulture)}   {dateTime.ToString("t", CultureInfo.CurrentCulture)}";
        }

        private string FormatDateString(DateTime dateTime)
        {
            return $"{dateTime.Date.ToString("d MMM yyyy", CultureInfo.CurrentCulture)}";
        }

        private void SetDateOnlyPicker(DateTime dateTime)
        {
            datePicker = new UIDatePickerStyled
            {
                Mode = UIDatePickerMode.Date
            };

            DateTextField.InputView = datePicker;
            SetDatePicker(dateTime);
        }

        private void SetDateAndTimePicker(DateTime dateTime)
        {
            datePicker = new UIDatePickerStyled
            {
                Mode = UIDatePickerMode.DateAndTime
            };
            DateTextField.InputView = datePicker;
            SetDatePicker(dateTime);
        }

        private void SetDatePicker(DateTime dateTime)
        {
            var fromComponents = new NSDateComponents
            {
                Day = dateTime.Day,
                Month = dateTime.Month,
                Year = dateTime.Year,
                Hour = dateTime.Hour,
                Minute = dateTime.Minute
            };

            var nsDate = NSCalendar.CurrentCalendar.DateFromComponents(fromComponents);
            datePicker.SetDate(nsDate, false);
        }

        [Export("doneTapped:")]
        void DoneTapped(UIBarButtonItem sender)
        {
            DateTextField.ResignFirstResponder();
            var selectedDate = datePicker.Date;
            var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year | NSCalendarUnit.Hour | NSCalendarUnit.Minute, selectedDate);
            var dateTime = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, (int)selectedDateComponents.Hour, (int)selectedDateComponents.Minute, 0, DateTimeKind.Local);
            SetDateAndTime(dateTime);
            DateChanged?.Invoke(new DateTimeChangeEvent(dateTime, RowType));
        }

        [Export("cancelTapped:")]
        void CancelTapped(UIBarButtonItem sender)
        {
            DateTextField.ResignFirstResponder();
        }

    }
}

