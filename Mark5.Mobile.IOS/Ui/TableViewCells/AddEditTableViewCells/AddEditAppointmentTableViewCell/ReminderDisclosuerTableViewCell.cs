using UIKit;
using System;
using Foundation;
using ObjCRuntime;
using CoreGraphics;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
{
    public class ReminderDisclosureTableViewCell : AppointmentDisclosureTableViewCell
    {
        Source pickerDataSource;
        UIPickerView pickerView;

        public Action<ReminderInfo> ReminderSelected;

        public ReminderDisclosureTableViewCell()
        {
            pickerDataSource = new Source();
            pickerView = new UIPickerView
            {
                DataSource = pickerDataSource,
                Delegate = pickerDataSource
            };

            HiddenTextView.InputView = pickerView;

            UIToolbar toolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
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

            HiddenTextView.InputAccessoryView = toolbar;
        }

        public void SetPickerSelection(ReminderInfo info)
        {
            var index = Array.FindIndex(pickerDataSource.reminders, reminder => reminder.Type == info.Type);
            pickerView.Select(index, 0, true);
        }

        [Export("doneTapped:")]
        void DoneTapped(UIBarButtonItem sender)
        {
            ReminderInfo reminder = pickerDataSource.GetSelectedReminder(pickerView);
            Label.Text = reminder.Title;
            ReminderSelected?.Invoke(reminder);
            HiddenTextView.ResignFirstResponder();
        }

        [Export("cancelTapped:")]
        void CancelTapped(UIBarButtonItem sender)
        {
            HiddenTextView.ResignFirstResponder();
        }

        public void ShowPicker()
        {
            HiddenTextView.BecomeFirstResponder();
        }

        class Source : UIPickerViewDataSource, IUIPickerViewDelegate
        {
            public readonly ReminderInfo[] reminders = {
                new ReminderInfo(ReminderInfo.ReminderType.None),
                new ReminderInfo(ReminderInfo.ReminderType.AtTheTime),
                new ReminderInfo(ReminderInfo.ReminderType.FiveMinutes),
                new ReminderInfo(ReminderInfo.ReminderType.FifteenMinutes),
                new ReminderInfo(ReminderInfo.ReminderType.ThirtyMinutes),
                new ReminderInfo(ReminderInfo.ReminderType.OneHour),
                new ReminderInfo(ReminderInfo.ReminderType.TwoHours),
                new ReminderInfo(ReminderInfo.ReminderType.OneDay)
            };

            public override nint GetComponentCount(UIPickerView pickerView)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
            {
                return reminders.Length;
            }

            [Export("pickerView:titleForRow:forComponent:")]
            public string GetTitle(UIPickerView picker, nint row, nint component)
            {
                return reminders[row].Title;
            }

            public ReminderInfo GetSelectedReminder(UIPickerView picker)
            {
                var selectedIndex = picker.SelectedRowInComponent(0);
                return reminders[selectedIndex];
            }
        }
    }
}
