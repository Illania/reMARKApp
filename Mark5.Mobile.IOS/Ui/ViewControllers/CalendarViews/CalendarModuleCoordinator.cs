using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CoreAnimation;
using Foundation;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Syncfusion.SfSchedule.iOS;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class CalendarModuleCoordinator : ICalendarView, ICalendarCoordinator, ICalendarListCoordinator
    {
        CalendarPresenter presenter;
        MonthViewController monthViewController;
        UICache uiCache;

        public NavigationController RootController { get; }
        public ObservableCollection<Appointment> Items => uiCache.Items;

        public CalendarModuleCoordinator()
        {
            uiCache = new UICache(this);
            monthViewController = new MonthViewController(this);

            RootController = new NavigationController(monthViewController);
        }

        #region ICalendarView

        public void UpdateAppointments(IEnumerable<AppointmentPreviewViewModel> caViewModels, DateTime start, DateTime end)
        {
            uiCache.CacheAppointments(caViewModels, start, end);
        }

        public void CalendarsSelected(List<CalendarViewModel> calendars)
        {
            uiCache.SetCalendars(calendars);
        }

        public void ShowLoading()
        {

        }

        public void StopLoading()
        {

        }

        public Task ShowError(Exception ex)
        {
            return Task.CompletedTask;
        }

        public void ShowAppointment(int appointmentId)
        {

        }

        public void ShowCalendarsList(Dictionary<CalendarViewModel, bool> calendars)
        {
            var calList = new CalendarsListViewController(this, calendars);
            RootController.PushViewController(calList, true);
        }

        public void AppointmentTapped(ScheduleAppointment appointment)
        {
            var appointmentId = int.Parse(appointment.Notes.ToString());
            presenter.AppointmentClicked(appointmentId);
        }

        #endregion

        #region ICalendarCoordinator

        public void VisibleDatesChanged(NSDate startDate, NSDate endDate)
        {
            var start = ((DateTime)startDate).ToLocalTime();
            var end = ((DateTime)endDate).ToLocalTime();

            presenter.LoadAppointments(start, end);
        }

        public void DateDoubleTapped(NSDate date)
        {
            RootController.PushViewController(new DayWeekViewController(this, date), true);
        }

        public void CreateAppointmentClicked()
        {
            RootController.PushViewController(new CreateAppointmentViewController(), true);
        }

        public void MonthTapped(NSDate date)
        {
            CATransition transition = new CATransition
            {
                Duration = 0.30,
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Default),
                Type = CAAnimation.TransitionPush,
                Subtype = CAAnimation.TransitionFromRight,
            };

            RootController.View.Layer.AddAnimation(transition, null);
            RootController.PopViewController(false);

            monthViewController.MoveToDate(date);
        }

        public void MonthViewLoaded()
        {
            presenter = new CalendarPresenter();
            presenter.AttachView(this);
            presenter.Start();
        }

        public void YearTapped(NSDate date)
        {
            YearViewController yearSelection = new YearViewController(this, date);
            CATransition transition = new CATransition
            {
                Duration = 0.35,
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear),
                Type = CAAnimation.TransitionPush,
                Subtype = CAAnimation.TransitionFromLeft,
            };

            RootController.View.Layer.AddAnimation(transition, null);
            RootController.PushViewController(yearSelection, false);
        }

        public void HourTapped(NSDate date)
        {
            //Create appointment view with start date set;
        }

        public void CalendarsClicked()
        {
            presenter.ShowCalendarsListClicked();
        }

        #endregion

        #region ICalendarListCoordinator implementation

        public void DoneButtonClicked(Dictionary<CalendarViewModel, bool> selectedCalendars)
        {
            var newSelectedState = selectedCalendars.ToDictionary(pair => pair.Key.Id, pair => pair.Value);
            presenter.CalendarSelectionChanged(newSelectedState);
            RootController.PopViewController(true);
        }

        public void CancelButtonClicked()
        {
            RootController.PopViewController(true);
        }

        #endregion

        class UICache
        {
            public ObservableCollection<Appointment> Items { get; } = new ObservableCollection<Appointment>();

            CalendarModuleCoordinator coordinator;

            List<AppointmentPreviewViewModel> AppointmentViewModels { get; } = new List<AppointmentPreviewViewModel>();
            List<CalendarViewModel> CalendarViewModels { get; } = new List<CalendarViewModel>();

            public UICache(CalendarModuleCoordinator coordinator)
            {
                this.coordinator = coordinator;
            }

            public void CacheAppointments(IEnumerable<AppointmentPreviewViewModel> appointmentViewModels, DateTime start, DateTime end)
            {
                AppointmentViewModels.Where(i => AppointmentIsInPeriod(i, start, end)).ToList().ForEach((obj) => AppointmentViewModels.Remove(obj));  //TODO need to test
                AppointmentViewModels.AddRange(appointmentViewModels);

                coordinator.RootController.BeginInvokeOnMainThread(() =>
                {
                    Items.Where(i => AppointmentIsInPeriod(i, start, end)).ToList().ForEach((obj) => Items.Remove(obj));  //TODO this can probably be done in a more clever way...
                    foreach (var caViewModel in AppointmentsInSelectedCalendars(appointmentViewModels))
                    {
                        Items.Add(Convert(caViewModel));
                    }
                });
            }

            public void UpdateSchedule()
            {
                if (!AppointmentViewModels.Any())
                    return;

                coordinator.RootController.BeginInvokeOnMainThread(() =>
                {
                    Items.Clear();
                    foreach (var item in AppointmentsInSelectedCalendars(AppointmentViewModels))
                    {
                        Items.Add(Convert(item));
                    }
                });
            }

            public void SetCalendars(List<CalendarViewModel> calendars)
            {
                CalendarViewModels.Clear();
                CalendarViewModels.AddRange(calendars);

                UpdateSchedule();
            }

            public void Clear()
            {
                CalendarViewModels.Clear();
                AppointmentViewModels.Clear();
                Items.Clear();
            }

            #region Utilities

            IEnumerable<AppointmentPreviewViewModel> AppointmentsInSelectedCalendars(IEnumerable<AppointmentPreviewViewModel> appointmentViewModels)
            {
                var calendarIds = new HashSet<int>(CalendarViewModels.Select(c => c.Id));
                return appointmentViewModels.Where(apvm => calendarIds.Contains(apvm.CalendarId));
            }

            bool AppointmentIsInPeriod(AppointmentPreviewViewModel appointment, DateTime start, DateTime end)
            {
                var appStart = appointment.Start;
                var appEnd = appointment.End;

                return DateTimeInPeriod(appStart, appEnd, start, end);
            }

            bool AppointmentIsInPeriod(Appointment appointment, DateTime start, DateTime end)
            {
                var appStart = (DateTime)appointment.Start;
                var appEnd = (DateTime)appointment.End;

                return DateTimeInPeriod(appStart, appEnd, start, end);
            }

            bool DateTimeInPeriod(DateTime appStart, DateTime appEnd, DateTime periodStart, DateTime periodEnd)
            {
                return appEnd > periodStart && appStart < periodEnd;
            }

            protected Appointment Convert(AppointmentPreviewViewModel cavm)
            {
                return new Appointment
                {
                    Subject = new NSString(cavm.Subject),
                    AllDay = cavm.AllDay,
                    Start = (NSDate)DateTime.SpecifyKind(cavm.Start, DateTimeKind.Local),
                    End = (NSDate)DateTime.SpecifyKind(cavm.End, DateTimeKind.Local),
                    Color = UI.UIColorFromHexString(cavm.HexColor),
                    Id = new NSString(cavm.Id.ToString())
                };
            }

            #endregion
        }
    }

    public class Appointment
    {
        public int CalendarId { get; set; }
        public bool AllDay { get; set; }
        public NSString Id { get; set; }
        public NSString Subject { get; set; }
        public NSDate Start { get; set; }
        public NSDate End { get; set; }
        public UIColor Color { get; set; }
    }

    public interface ICalendarCoordinator
    {
        ObservableCollection<Appointment> Items { get; }

        void VisibleDatesChanged(NSDate startDate, NSDate endDate);
        void DateDoubleTapped(NSDate date);
        void MonthTapped(NSDate date);
        void CreateAppointmentClicked();
        void MonthViewLoaded();
        void YearTapped(NSDate nSDate);
        void CalendarsClicked();
        void AppointmentTapped(ScheduleAppointment appointment);
        void HourTapped(NSDate date);
    }

    public interface ICalendarListCoordinator
    {
        void DoneButtonClicked(Dictionary<CalendarViewModel, bool> selectedCalendars);
        void CancelButtonClicked();
    }
}
