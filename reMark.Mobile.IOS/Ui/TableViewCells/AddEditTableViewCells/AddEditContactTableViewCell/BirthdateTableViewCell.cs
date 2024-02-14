using System;
using CoreGraphics;
using Foundation;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;
using Contact = reMark.Mobile.Common.Model.Contact;

namespace reMark.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells
{
    public class BirthdateTableViewCell : MultiRowContentTableViewCell
    {
        public static readonly NSString Key = new NSString("BirthdateTableViewCell");

        Contact contact;

        readonly UITextFieldScalable dateTextField;
        readonly UIToolbar datePickerToolbar;
        readonly UIBarButtonItem dateDoneButton;
        readonly UIDatePickerStyled datePicker;

        public BirthdateTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            datePickerToolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
            {
                Items = new[]
                {
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        dateDoneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, this, new Selector("doneTapped:"))
                        {
                            TintColor = Theme.DarkerBlue
                        }
                    }
            };

            datePicker = new UIDatePickerStyled
            {
                Mode = UIDatePickerMode.Date,
                MinimumDate = null,
                MaximumDate = NSDate.Now
            };

            dateTextField = new UITextFieldScalable
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont.CustomFont(),
                BorderStyle = UITextBorderStyle.None,
                InputView = datePicker,
                InputAccessoryView = datePickerToolbar,
            };

            ContentView.AddSubview(dateTextField);

            ContentView.AddConstraints(new[]
            {
                dateTextField.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, VerticalMargin),
                dateTextField.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, - VerticalMargin),
                dateTextField.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                dateTextField.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
            });
        }

        public void BindContact(Contact contact)
        {
            SetErrorState(false);

            this.contact = contact;
            RefreshDate();
        }

        public void StartEditing()
        {
            dateTextField.BecomeFirstResponder();
        }

        void RefreshDate()
        {
            if (contact.BirthDateTimestamp != -1)
            {
                var date = contact.BirthDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
                var components = new NSDateComponents
                {
                    Day = date.Day,
                    Month = date.Month,
                    Year = date.Year,
                    TimeZone = NSTimeZone.FromName("UTC")
                };

                var fromNSDate = NSCalendar.CurrentCalendar.DateFromComponents(components);
                dateTextField.Text = DateTimeFormatter.LongDateFormatter.StringFor(fromNSDate);

                datePicker.SetDate(fromNSDate, false);
            }
            else
            {
                dateTextField.Text = string.Empty;
            }
        }

        [Export("doneTapped:")]
        void DoneTapped(UIBarButtonItem sender)
        {
            var selectedDate = datePicker.Date;
            var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year, selectedDate);
            var birthdate = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, 12, 00, 00, DateTimeKind.Utc);

            contact.BirthDateTimestamp = birthdate.ConvertServerTimeToUtc().ConvertDateTimeToTimestampMilliseconds();
            RefreshDate();
            dateTextField.ResignFirstResponder();
        }
    }
}
