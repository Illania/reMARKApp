using System;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public class BirthdateTableViewCell : MultiRowContentTableViewCell
    {
        public static readonly NSString Key = new NSString("BirthdateTableViewCell");

        Contact contact;

        readonly UITextField dateTextField;
        readonly UIToolbar datePickerToolbar;
        readonly UIBarButtonItem dateDoneButton;
        readonly UIDatePicker datePicker;

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

            datePicker = new UIDatePicker
            {
                Mode = UIDatePickerMode.Date,
                MinimumDate = null,
                MaximumDate = NSDate.Now,
            };

            dateTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = Theme.DefaultFont,
                BorderStyle = UITextBorderStyle.None,
                InputView = datePicker,
                InputAccessoryView = datePickerToolbar,
            };

            ContentView.AddSubview(dateTextField);

            ContentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(dateTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.TopMargin, 1f, VerticalMargin),
                NSLayoutConstraint.Create(dateTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.LeftMargin, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(dateTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.RightMargin, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(dateTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.BottomMargin, 1f, -VerticalMargin),
            });
        }

        public void BindContact(Contact contact)
        {
            SetErrorState(false);

            this.contact = contact;
            RefreshDate();
        }

        public override void StartEditing()
        {
            dateTextField.BecomeFirstResponder();
        }

        //TODO need to find a way to have a date anyway

        void RefreshDate()
        {
            if (contact.BirthDateTimestamp != -6847804800000 && contact.BirthDateTimestamp != -1)
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

            contact.BirthDateTimestamp = birthdate.ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();
            RefreshDate();
            dateTextField.ResignFirstResponder();
        }
    }
}
