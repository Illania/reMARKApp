using UIKit;
using Syncfusion.SfSchedule.iOS;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using System.Collections.ObjectModel;
using Foundation;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class DayWeekViewController : CalendarViewController
    {
        UIBarButtonItem addButtonItem;
        UIBarButtonItem switchButtonItem;

        readonly NSDate date;

        public DayWeekViewController(NSDate date, ObservableCollection<Meeting> itemsSource)
        {
            this.date = date;
        }

        public override void LoadView()
        {
            base.LoadView();

            schedule = new DayWeekSchedule();
            MoveToDate(date);

            View.BackgroundColor = Theme.White;
            View.AddSubview(schedule);
            View.AddConstraints(new NSLayoutConstraint[] {
                schedule.TopAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.TopAnchor : View.TopAnchor),
                schedule.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : View.BottomAnchor),
                schedule.RightAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.RightAnchor : View.RightAnchor),
                schedule.LeftAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.LeftAnchor : View.LeftAnchor)
            });

            InitializeNavigationBar();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();
        }

        void InitializeHandlers()
        {
            if (schedule != null)
            {
                schedule.CellLongPressed += ReMarkDayCalendar_CellTapped;
                schedule.CellDoubleTapped += ReMarkDayCalendar_CellTapped;
                schedule.VisibleDatesChanged += Schedule_VisibleDatesChanged;
            }

            if (addButtonItem != null)
                addButtonItem.Clicked += AddButtonItem_Clicked;

            if (switchButtonItem != null)
                switchButtonItem.Clicked += ScheduleSwitchBtn_Clicked;
        }

        void DeInitializeHandlers()
        {
            if (schedule != null)
            {
                schedule.CellLongPressed -= ReMarkDayCalendar_CellTapped;
                schedule.CellDoubleTapped -= ReMarkDayCalendar_CellTapped;
                schedule.VisibleDatesChanged -= Schedule_VisibleDatesChanged;
            }

            if (addButtonItem != null)
                addButtonItem.Clicked -= AddButtonItem_Clicked;

            if (switchButtonItem != null)
                switchButtonItem.Clicked -= ScheduleSwitchBtn_Clicked;
        }

        void ReMarkDayCalendar_CellTapped(object sender, CellTappedEventArgs e)
        {
            if (e != null)
            {
                //NavigationController.PushViewController(new AppointmentViewController(), true);
            }
        }

        void InitializeNavigationBar()
        {
            addButtonItem = new UIBarButtonItem
            {
                Title = "",
                Image = UIImage.FromBundle("Create")
            };

            switchButtonItem = new UIBarButtonItem();

            NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { addButtonItem, switchButtonItem }, false);
            UpdateSwitchButtonTitle();
        }

        #region CalendarView implementation

        public override void SetCalendars(List<CalendarViewModel> calendars)
        {
            throw new NotImplementedException();
        }

        public override void UpdateAppointments(IEnumerable<SimpleCalendarAppointmentViewModel> caViewModels, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public override void ShowLoading()
        {
            throw new NotImplementedException();
        }

        public override void StopLoading()
        {
            throw new NotImplementedException();
        }

        public override Task ShowError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public override void ShowAppointment(int appointmentId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Event handlers

        void UpdateSwitchButtonTitle()
        {
            switchButtonItem.Title = schedule.ScheduleView == SFScheduleView.SFScheduleViewWeek ?
                    Localization.GetString("day") : Localization.GetString("week");  //TODO localization
        }

        void Schedule_VisibleDatesChanged(object sender, VisibleDatesChangedEventArgs e)
        {
        }


        void AddButtonItem_Clicked(object sender, System.EventArgs e)
        {
            var newAppointmentVC = new CreateAppointmentViewController();
            NavigationController.PushViewController(newAppointmentVC, true);
        }

        void ScheduleSwitchBtn_Clicked(object sender, System.EventArgs e)
        {
            if (schedule.ScheduleView == SFScheduleView.SFScheduleViewWorkWeek)
                schedule.ScheduleView = SFScheduleView.SFScheduleViewDay;
            else
                schedule.ScheduleView = SFScheduleView.SFScheduleViewWorkWeek;

            UpdateSwitchButtonTitle();
        }

        #endregion
    }

    class DayWeekSchedule : SFSchedule
    {
        readonly HeaderStyle headerStyle = new HeaderStyle
        {
            BackgroundColor = Theme.DarkerBlue,
            TextColor = UIColor.White,
            TextStyle = Theme.DefaultLightFont,
            TextPosition = UITextAlignment.Center
        };

        readonly SFViewHeaderStyle viewHeaderStyle = new SFViewHeaderStyle
        {
            BackgroundColor = Theme.DarkerBlue,
            DateTextStyle = Theme.DefaultLightFont,
            DateTextColor = UIColor.White,
            CurrentDateTextColor = UIColor.White,
            CurrentDayTextColor = UIColor.White,
            DayTextColor = UIColor.White,
            DayTextStyle = Theme.DefaultLightFont
        };

        readonly DayViewSettings dayViewSettings = new DayViewSettings
        {
            LabelSettings = new DayLabelSettings
            {
                TimeLabelColor = Theme.DarkGray
            }
        };

        readonly SFViewHeaderStyle workWeekDayHeader = new SFViewHeaderStyle()
        {
            DayTextColor = UIColor.White,
            DayTextStyle = Theme.DefaultLightFont,
            DateTextColor = UIColor.White,
            DateTextStyle = Theme.DefaultLightFont,
            BackgroundColor = Theme.DarkerBlue,
            CurrentDayTextColor = UIColor.White
        };

        readonly WeekViewSettings workWeekSettings = new WeekViewSettings()
        {
            LabelSettings = new WeekLabelSettings()
            {
                TimeLabelColor = Theme.DarkGray
            }
        };

        readonly SFAppointmentStyle appointmentStyle = new SFAppointmentStyle()
        {
            TextStyle = UIFont.FromName("Avenir-Light", 14),
            SelectionBorderColor = Theme.DarkerBlue
        };

        public DayWeekSchedule()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            ScheduleView = SFScheduleView.SFScheduleViewDay;
            DayHeaderStyle = workWeekDayHeader;
            WeekViewSettings = workWeekSettings;
            HeaderStyle = headerStyle;
            DayViewSettings = dayViewSettings;
            DayHeaderStyle = viewHeaderStyle;
            AppointmentStyle = appointmentStyle;
        }
    }
}
