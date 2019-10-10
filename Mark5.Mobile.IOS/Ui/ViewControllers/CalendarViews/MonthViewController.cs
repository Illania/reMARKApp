using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Syncfusion.SfSchedule.iOS;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class MonthViewController : CalendarViewController
    {
        UIBarButtonItem yearButton;
        UIBarButtonItem calendarsButton;
        UIBarButtonItem createAppointmentsButton;
        UIBarButtonItem refreshButton;

        public MonthViewController(ICalendarCoordinator coordinator) : base(coordinator) { }

        public override void LoadView()
        {
            base.LoadView();

            Coordinator.MonthViewLoaded();

            schedule = new MonthSchedule();
            View.BackgroundColor = UIColor.White;
            View.AddSubview(schedule);
            View.AddConstraints(new NSLayoutConstraint[] {
                schedule.TopAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.TopAnchor : View.TopAnchor),
                schedule.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : View.BottomAnchor),
                schedule.RightAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.RightAnchor : View.RightAnchor),
                schedule.LeftAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.LeftAnchor : View.LeftAnchor)
            });

            InitializeNavigationBar();

            MoveToDate(NSDate.Now);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBarHidden = false;
            NavigationController.Title = string.Empty;

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
                schedule.HeaderTapped += Handle_HeaderTapped;
                schedule.CellDoubleTapped += Schedule_CellDoubleTapped;
                schedule.MonthInlineAppointmentTapped += Schedule_MonthInlineAppointmentTapped;
                schedule.VisibleDatesChanged += MonthSchedule_VisibleDatesChanged;
            }

            if (yearButton != null)
                yearButton.Clicked += YearButtonItem_Clicked;

            if (createAppointmentsButton != null)
                createAppointmentsButton.Clicked += CreateAppointmentButtonItem_Clicked;

            if (calendarsButton != null)
                calendarsButton.Clicked += CalendarsButtonItem_Clicked;

            if (refreshButton != null)
                refreshButton.Clicked += RefreshButton_Clicked;
        }

        void DeInitializeHandlers()
        {
            if (schedule != null)
            {
                schedule.HeaderTapped -= Handle_HeaderTapped;
                schedule.CellDoubleTapped -= Schedule_CellDoubleTapped;
                schedule.CellLongPressed -= Schedule_CellDoubleTapped;
                schedule.MonthInlineAppointmentTapped -= Schedule_MonthInlineAppointmentTapped;
                schedule.VisibleDatesChanged -= MonthSchedule_VisibleDatesChanged;
            }

            if (yearButton != null)
                yearButton.Clicked -= YearButtonItem_Clicked;

            if (createAppointmentsButton != null)
                createAppointmentsButton.Clicked -= CreateAppointmentButtonItem_Clicked;

            if (calendarsButton != null)
                calendarsButton.Clicked -= CalendarsButtonItem_Clicked;

            if (refreshButton != null)
                refreshButton.Clicked -= RefreshButton_Clicked;
        }

        void InitializeNavigationBar()
        {
            yearButton = new UIBarButtonItem
            {
                Title = Localization.GetString("year"),
            };

            NavigationItem.SetLeftBarButtonItem(yearButton, true);

            createAppointmentsButton = new UIBarButtonItem
            {
                Title = Localization.GetString("create"),
                Image = UIImage.FromBundle("Create")
            };

            calendarsButton = new UIBarButtonItem
            {
                Title = Localization.GetString("calendars"),
            };

            refreshButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh)
            {
                Title = Localization.GetString("refresh")
            };

            NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { createAppointmentsButton, calendarsButton, refreshButton }, true);
        }

        #region Event handlers

        void Schedule_MonthInlineAppointmentTapped(object sender, MonthInlineAppointmentTappedEventArgs e)
        {
            Coordinator.AppointmentTapped(e.Appointment);
        }

        void MonthSchedule_VisibleDatesChanged(object sender, VisibleDatesChangedEventArgs e)
        {
            var startDate = schedule.VisibleDates.GetItem<NSDate>(0);
            var endDate = schedule.VisibleDates.GetItem<NSDate>(schedule.VisibleDates.Count - 1);

            Coordinator.VisibleDatesChanged(startDate, endDate);
        }

        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            Coordinator.DateDoubleTapped(e.Date);
        }

        void Handle_HeaderTapped(object sender, HeaderTappedEventArgs e)
        {
            Coordinator.YearTapped(schedule.VisibleDates.GetItem<NSDate>(0));
        }

        void Schedule_ViewHeaderTapped(object sender, ViewHeaderTappedEventArgs e)
        {
            Coordinator.YearTapped(schedule.VisibleDates.GetItem<NSDate>(0));
        }

        void YearButtonItem_Clicked(object sender, EventArgs e)
        {
            Coordinator.YearTapped(schedule.VisibleDates.GetItem<NSDate>(0));
        }

        void CreateAppointmentButtonItem_Clicked(object sender, EventArgs e)
        {
            Coordinator.CreateAppointmentClicked();
        }

        void CalendarsButtonItem_Clicked(object sender, EventArgs e)
        {
            Coordinator.CalendarsClicked();
        }

        void RefreshButton_Clicked(object sender, EventArgs e)
        {
            var startDate = schedule.VisibleDates.GetItem<NSDate>(0);
            var endDate = schedule.VisibleDates.GetItem<NSDate>(schedule.VisibleDates.Count - 1);

            Coordinator.RefreshClicked(startDate, endDate);
        }

        #endregion
    }

    class MonthSchedule : SFSchedule
    {
        public MonthSchedule()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;

            FirstDayOfWeek = 2;
            ScheduleView = SFScheduleView.SFScheduleViewMonth;
            EnableNavigation = true;

            MonthViewSettings = new MonthViewSettings
            {
                ShowAgendaView = true,
                ShowAppointmentsInline = false,
                TodayBackgroundColor = Theme.LightBlue,
                SelectionIndicatorColor = Theme.DarkBlue,
                SelectionTextColor = Theme.White,
                AgendaViewStyle = new AgendaViewStyle
                {
                    SubjectTextColor = Theme.DarkerBlue,
                    SubjectTextStyle = Theme.DefaultLightFont,
                    TimeTextStyle = Theme.CalendarTimeLightFont,
                    TimeTextColor = Theme.DarkGray,
                    DateTextColor = Theme.DarkGray,
                    DateTextStyle = Theme.DefaultLightFont,
                    HeaderHeight = 50f
                }
            };

            HeaderStyle = new HeaderStyle
            {
                TextColor = Theme.DarkerBlue,
            };

            DayHeaderStyle = new SFViewHeaderStyle
            {
                DayTextColor = Theme.DarkerBlue,
                DayTextStyle = Theme.DefaultLightFont
            };
        }
    }
}
