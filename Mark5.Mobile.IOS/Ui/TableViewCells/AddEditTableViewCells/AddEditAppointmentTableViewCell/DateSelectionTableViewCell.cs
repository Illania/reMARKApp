using UIKit;
using System;
using System.Globalization;
using Foundation;
using ObjCRuntime;
using CoreGraphics;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Ui.Common;
using static Mark5.Mobile.IOS.Model.DateTimeChangeEvent;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
{
    public class DateSelectioTableViewCell : AddEditTableViewCell
    {
        static readonly string Key = "DateSelectioTableViewCell";
        readonly DateRowType rowType;

        UIDatePicker datePicker;

        public Action<DateTimeChangeEvent> DateChanged = delegate { };
        public UITextField DateTextField;
        public UILabel Label;

        public DateSelectioTableViewCell(Action<DateTimeChangeEvent> dateChanged, DateRowType rowType) : base(UITableViewCellStyle.Default, Key)
        {
            this.rowType = rowType;
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            DateChanged += dateChanged;

            Label = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont
            };
            ContentView.Add(Label);

            datePicker = new UIDatePicker
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

            DateTextField = new UITextField
            {
                Font = Theme.DefaultFont,
                TintColor = Theme.Clear,
                TextAlignment = UITextAlignment.Right,
                InputView = datePicker,
                InputAccessoryView = datePickerToolbar,
                TranslatesAutoresizingMaskIntoConstraints = false,
                UserInteractionEnabled = true,
                Text = string.Empty
            };

            ContentView.Add(DateTextField);

            ContentView.AddConstraints(new[]
            {
                Label.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                Label.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor),
                Label.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),

                DateTextField.HeightAnchor.ConstraintGreaterThanOrEqualTo(20f),
                DateTextField.TopAnchor.ConstraintEqualTo(Label.TopAnchor),
                DateTextField.BottomAnchor.ConstraintEqualTo(Label.BottomAnchor),
                DateTextField.LeadingAnchor.ConstraintGreaterThanOrEqualTo(Label.LeadingAnchor, HorizontalMargin),
                DateTextField.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor)
            });
        }

        public void SetDateOnly(DateTime dateTime)
        {
            UIStringAttributes attributes = new UIStringAttributes(new NSDictionary(
                UIStringAttributeKey.Font, Theme.DefaultFont,
                UIStringAttributeKey.StrikethroughStyle, NSUnderlineStyle.None
            ));

            NSMutableAttributedString prettyString = new NSMutableAttributedString(FormatDateString(dateTime));

            prettyString.SetAttributes(attributes.Dictionary, new NSRange(0, prettyString.Length));

            DateTextField.AttributedText = prettyString;
            DateTextField.TextColor = UIColor.Black;

            SetDateOnlyPicker(dateTime);
        }

        public void SetDateAndTime(DateTime dateTime)
        {
            UIStringAttributes attributes = new UIStringAttributes(new NSDictionary(
                UIStringAttributeKey.Font, Theme.DefaultFont,
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
            return $"{ dateTime.ToString("d MMM yyyy", CultureInfo.CurrentCulture) }   { dateTime.ToString("t", CultureInfo.CurrentCulture) }";
        }

        private string FormatDateString(DateTime dateTime)
        {
            return $"{ dateTime.Date.ToString("d MMM yyyy", CultureInfo.CurrentCulture) }";
        }

        private void SetDateOnlyPicker(DateTime dateTime)
        {
            datePicker = new UIDatePicker
            {
                Mode = UIDatePickerMode.Date
            };

            DateTextField.InputView = datePicker;
            SetDatePicker(dateTime);
        }

        private void SetDateAndTimePicker(DateTime dateTime)
        {
            datePicker = new UIDatePicker
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
            DateChanged?.Invoke(new DateTimeChangeEvent(dateTime, rowType));
        }

        [Export("cancelTapped:")]
        void CancelTapped(UIBarButtonItem sender)
        {
            DateTextField.ResignFirstResponder();
        }
    }
}
