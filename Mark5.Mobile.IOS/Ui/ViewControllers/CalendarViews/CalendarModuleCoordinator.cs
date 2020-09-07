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
using Xamarin.Essentials;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class CalendarModuleCoordinator : ICalendarView, ICalendarCoordinator, ICalendarListCoordinator
    {
        CalendarPresenter presenter;
        Action loadingDialogDismissal;

        readonly MonthViewController monthViewController;
        DayWeekViewController dayWeekViewController;
        readonly UICache uiCache;

        public NavigationController RootController { get; }
        public ObservableCollection<Appointment> Items => uiCache.Items;

        public CalendarModuleCoordinator()
        {
            uiCache = new UICache();
            uiCache.SourceUpdated += UiCache_SourceUpdated;
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

        public void StopLoading()
        {
            loadingDialogDismissal?.Invoke();
        }

        public void ShowLoading()
        {
            loadingDialogDismissal = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_appointments___"));
        }

        public Task ShowError(Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
           {
               await Dialogs.ShowErrorAlertAsync(RootController, ex);
           });

            return Task.CompletedTask;
        }

        public void ShowAppointment(int calendarId, int appointmentId, int recurrenceIndex)
        {
            RootController.PushViewController(new AppointmentViewController(calendarId, appointmentId, recurrenceIndex), true);
        }

        public void ShowCalendarsList(Dictionary<CalendarViewModel, bool> calendars)
        {
            RootController.PushViewController(new CalendarsListViewController(this, calendars), true);
        }

        public void DeleteAppointmentsWithIds(List<int> appointmentIds)
        {
            uiCache.DeleteAppointmentsWithIds(appointmentIds);
        }

        #endregion

        #region ICalendarCoordinator

        public void AppointmentTapped(ScheduleAppointment appointment)
        {
            var splitted = appointment.Notes.ToString().Split(" ");
            var calendarId = int.Parse(splitted[0]);
            var appointmentId = int.Parse(splitted[1]);
            var recurrenceIndex = int.Parse(splitted[2]);
            presenter.AppointmentClicked(calendarId, appointmentId, recurrenceIndex);
        }

        public void VisibleDatesChanged(NSDate startDate, NSDate endDate)
        {
            var start = ((DateTime)startDate).ToLocalTime();
            var end = ((DateTime)endDate).ToLocalTime();

            presenter.LoadAppointments(start, end);
        }

        public void DateDoubleTapped(NSDate date)
        {
            dayWeekViewController = new DayWeekViewController(this, date);
            RootController.PushViewController(dayWeekViewController, true);
        }

        public void CreateAppointmentClicked(NSDate date)
        {
            RootController.PushViewController(new AddAppointmentViewController(((DateTime)date).ToLocalTime()), true);
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

        public void ReleaseDayViewController()
        {
            dayWeekViewController = null;
        }

        public void YearTapped(NSDate date)
        {
            var yearSelection = new YearViewController(this, date);
            CATransition transition = new CATransition
            {
                Duration = 0.30,
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear),
                Type = CAAnimation.TransitionMoveIn,
                Subtype = CAAnimation.TransitionFromLeft,
            };

            RootController.View.Window.BackgroundColor = UIColor.White;
            RootController.View.Layer.BackgroundColor = UIColor.White.CGColor;


            RootController.View.Layer.AddAnimation(transition, null);
            RootController.PushViewController(yearSelection, true);
        }

        public void HourTapped(NSDate date)
        {
            var start = ((DateTime)date).ToLocalTime();
            RootController.PushViewController(new AddAppointmentViewController(start), true);
        }

        public void CalendarsClicked()
        {
            presenter.ShowCalendarsListClicked();
        }

        public void RefreshClicked(NSDate visibleStartDate, NSDate visibleEndDate)
        {
            uiCache.Clear();

            var start = ((DateTime)visibleStartDate).ToLocalTime();
            var end = ((DateTime)visibleEndDate).ToLocalTime();

            presenter.RefreshClicked(start, end);
        }

        private void UiCache_SourceUpdated(object sender, EventArgs e)
        {
            dayWeekViewController?.UpdateSource();
            monthViewController.UpdateSource();
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
            public event EventHandler SourceUpdated = delegate { };

            public ObservableCollection<Appointment> Items { get; private set; } = new ObservableCollection<Appointment>();

            List<AppointmentPreviewViewModel> AppointmentViewModels { get; } = new List<AppointmentPreviewViewModel>();
            List<CalendarViewModel> CalendarViewModels { get; } = new List<CalendarViewModel>();

            public void CacheAppointments(IEnumerable<AppointmentPreviewViewModel> appointmentViewModels, DateTime start, DateTime end)
            {
                AppointmentViewModels.Where(i => AppointmentIsInPeriod(i, start, end)).ToList().ForEach((obj) => AppointmentViewModels.Remove(obj));
                AppointmentViewModels.AddRange(appointmentViewModels);

                UpdateSchedule();
            }

            public void UpdateSchedule()
            {
                if (!AppointmentViewModels.Any())
                    return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Items.Clear();
                    var newItems = new ObservableCollection<Appointment>();
                    foreach (var caViewModel in AppointmentsInSelectedCalendars(AppointmentViewModels))
                        newItems.Add(Convert(caViewModel));

                    Items = newItems;
                    SourceUpdated(this, EventArgs.Empty);
                });
            }

            public void SetCalendars(List<CalendarViewModel> calendars)
            {
                CalendarViewModels.Clear();
                CalendarViewModels.AddRange(calendars);

                UpdateSchedule();
            }

            public void DeleteAppointmentsWithIds(List<int> appointmentIds)
            {
                AppointmentViewModels.Where(i => appointmentIds.Contains(i.Id)).ToList().ForEach((obj) => AppointmentViewModels.Remove(obj));

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Items.Where(i => appointmentIds.Contains(GetAppointmentInfo(i).id)).ToList().ForEach((obj) => Items.Remove(obj));
                });
            }

            public void Clear()
            {
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

            (int calendarId, int id, int recurrenceIndex) GetAppointmentInfo(Appointment appointment)
            {
                var splitted = appointment.Id.ToString().Split(" ");
                return (int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]));
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
                    Id = new NSString($"{cavm.CalendarId} {cavm.Id} {cavm.RecurrenceIndex}")
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
        void CreateAppointmentClicked(NSDate date);
        void MonthViewLoaded();
        void ReleaseDayViewController();
        void YearTapped(NSDate nSDate);
        void CalendarsClicked();
        void AppointmentTapped(ScheduleAppointment appointment);
        void HourTapped(NSDate date);
        void RefreshClicked(NSDate visibleStartDate, NSDate visibleEndDate);
    }

    public interface ICalendarListCoordinator
    {
        void DoneButtonClicked(Dictionary<CalendarViewModel, bool> selectedCalendars);
        void CancelButtonClicked();
    }
}
