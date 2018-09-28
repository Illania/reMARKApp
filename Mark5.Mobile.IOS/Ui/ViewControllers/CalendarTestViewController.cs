using System;
using System.Collections.Generic;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Manager;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class CalendarTestViewController : AbstractViewController
    {
        public override void LoadView()
        {
            base.LoadView();

            InitializeStartView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = false;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;
            }
        }

        UIDatePicker fromDatePicker;
        UIDatePicker toDatePicker;
        UIPickerView calendarPicker;
        UIStackView internalStackView;
        UISwitch toggleControl;

        Calendar selectedCalendar;
        long selectedFromDateTime;
        long selectedToDateTime;

        void InitializeStartView()
        {
            var scrollView = new UIScrollView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            View.AddSubview(scrollView);
            View.AddConstraints(new[] {
                scrollView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                scrollView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.ReadableContentGuide.BottomAnchor)
            });

            var stackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical,
            };

            scrollView.AddSubview(stackView);
            scrollView.AddConstraints(new[] {
                stackView.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor),
                stackView.LeftAnchor.ConstraintEqualTo(scrollView.LeftAnchor),
                stackView.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor),
                stackView.RightAnchor.ConstraintEqualTo(scrollView.RightAnchor),
                stackView.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor)
            });

            fromDatePicker = new UIDatePicker()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            toDatePicker = new UIDatePicker()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            calendarPicker = new UIPickerView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            var model = new PickerSource();
            model.ValueChanged += Model_ValueChanged;
            calendarPicker.Model = model;

            var button = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            button.SetTitle("GO", UIControlState.Normal);
            button.SetTitleColor(UIColor.Purple, UIControlState.Normal);
            button.TouchUpInside += Button_TouchUpInside;

            var switchStackView = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            var textView = new UITextView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextColor = UIColor.DarkTextColor,
                ScrollEnabled = false,
            };

            textView.Text = "Appointment / Task";
            textView.Font = UIFont.SystemFontOfSize(textSize);

            toggleControl = new UISwitch()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            switchStackView.AddArrangedSubview(textView);
            switchStackView.AddArrangedSubview(toggleControl);

            internalStackView = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            stackView.AddArrangedSubview(calendarPicker);
            stackView.AddArrangedSubview(fromDatePicker);
            stackView.AddArrangedSubview(toDatePicker);
            stackView.AddArrangedSubview(switchStackView);
            stackView.AddArrangedSubview(button);
            stackView.AddArrangedSubview(internalStackView);

            selectedCalendar = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars[0];
        }

        void Model_ValueChanged(object sender, Calendar e)
        {
            selectedCalendar = e;
        }

        async void Button_TouchUpInside(object sender, EventArgs e)
        {
            var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year
                                                                   | NSCalendarUnit.Hour | NSCalendarUnit.Minute, fromDatePicker.Date);
            var fromDate = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, (int)selectedDateComponents.Hour
                                        , (int)selectedDateComponents.Minute, 0, DateTimeKind.Utc);
            selectedFromDateTime = fromDate.ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();


            var selectedDateComponents2 = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year
                                                                   | NSCalendarUnit.Hour | NSCalendarUnit.Minute, toDatePicker.Date);
            var toDate = new DateTime((int)selectedDateComponents2.Year, (int)selectedDateComponents2.Month, (int)selectedDateComponents2.Day, (int)selectedDateComponents2.Hour
                                        , (int)selectedDateComponents2.Minute, 0, DateTimeKind.Utc);
            selectedToDateTime = toDate.ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();

            try
            {
                if (toggleControl.On)
                {
                    var list = await Managers.CalendarManager.GetCalendarTasksAsync(new List<int> { selectedCalendar.Id }, selectedFromDateTime, selectedToDateTime);
                    AddAppointments(tasks: list);
                }
                else
                {
                    var list = await Managers.CalendarManager.GetCalendarAppointmentsAsync(new List<int> { selectedCalendar.Id }, selectedFromDateTime, selectedToDateTime);
                    AddAppointments(appointments: list);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);
            }
        }

        const int textSize = 16;

        void AddAppointments(List<CalendarAppointment> appointments = null, List<CalendarTask> tasks = null)
        {
            foreach (var subview in internalStackView.Subviews)
            {
                subview.RemoveFromSuperview();
            }

            if (appointments != null)
            {
                foreach (var appointment in appointments)
                {
                    var textView = new UITextView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        TextColor = UIColor.DarkTextColor,
                        ScrollEnabled = false,

                    };

                    var fromDate = appointment.StartDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToUserTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatUserTimestampAsTimeAndDateString();

                    var toDate = appointment.EndDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToUserTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatUserTimestampAsTimeAndDateString();

                    var text = $"APPOINTMENT: {appointment.Subject} {fromDate} {toDate}";
                    textView.Font = UIFont.SystemFontOfSize(textSize);

                    textView.Text = text;

                    internalStackView.AddArrangedSubview(textView);
                }
            }

            if (tasks != null)
            {
                foreach (var task in tasks)
                {
                    var textView = new UITextView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        TextColor = UIColor.DarkTextColor,
                        ScrollEnabled = false,

                    };

                    var fromDate = task.StartDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToUserTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatUserTimestampAsCompactShortDateTimeString();

                    var toDate = task.EndDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToUserTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatUserTimestampAsCompactShortDateTimeString();

                    var text = $"TASK: {task.Subject} {fromDate} {toDate}";
                    textView.Font = UIFont.SystemFontOfSize(textSize);

                    textView.Text = text;

                    internalStackView.AddArrangedSubview(textView);
                }
            }

            if ((appointments?.Count == 0 || appointments == null) && (tasks == null || tasks?.Count == 0))
            {
                var textView = new UITextView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    TextColor = UIColor.DarkTextColor,
                    ScrollEnabled = false,
                };

                textView.Text = "EMPTY";
                textView.Font = UIFont.SystemFontOfSize(textSize);

                internalStackView.AddArrangedSubview(textView);
            }
        }


        class PickerSource : UIPickerViewModel
        {
            List<Calendar> calendars = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;
            public event EventHandler<Calendar> ValueChanged;

            public override nint GetComponentCount(UIPickerView pickerView)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
            {
                return calendars.Count;
            }

            public override string GetTitle(UIPickerView pickerView, nint row, nint component)
            {
                return calendars[(int)row].Name;
            }

            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                ValueChanged(this, calendars[(int)row]);
            }
        }

    }
}
