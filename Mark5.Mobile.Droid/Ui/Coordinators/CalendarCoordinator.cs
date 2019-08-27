using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Support.V4.App;
using Android.Views;
using Com.Syncfusion.Schedule;
using Java.Util;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments.Calendar;
using Mark5.Mobile.Droid.Utilities;
using Xamarin.Essentials;

namespace Mark5.Mobile.Droid.Ui.Coordinators
{
    public class CalendarModuleCoordinator : ICalendarCoordinator, ICalendarView
    {
        readonly BaseAppCompatActivity activity;
        readonly FragmentManager fragmentManager;

        MonthCalendarFragment monthCalendarFragment;

        Action dismissAction;

        CalendarPresenter presenter;

        readonly UICache uiCache;

        public ObservableCollection<Appointment> Items => uiCache.Items;

        public CalendarModuleCoordinator(BaseAppCompatActivity a)
        {
            activity = a;
            fragmentManager = a.SupportFragmentManager;

            uiCache = new UICache();
        }

        public (BaseFragment, string) GetMainFragment() => (monthCalendarFragment, _) = MonthCalendarFragment.NewInstance();

        #region ICalendarView implementation

        public void CalendarsSelected(List<CalendarViewModel> calendars)
        {
            uiCache.SetCalendars(calendars);
        }

        public void ShowCalendarsList(Dictionary<CalendarViewModel, bool> calendars)
        {
            var (fragment, tag) = CalendarListFragment.NewInstance(calendars);
            fragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
        }

        public void UpdateAppointments(IEnumerable<AppointmentPreviewViewModel> caViewModels, DateTime start, DateTime end)
        {
            uiCache.CacheAppointments(caViewModels, start, end);
        }

        public void ShowLoading()
        {
            dismissAction = Dialogs.ShowInfiniteProgressDialog(activity, Resource.String.loading_appointments, Resource.String.please_wait);
        }

        public void StopLoading()
        {
            dismissAction?.Invoke();
        }

        public Task ShowError(Exception ex)
        {
            return Task.CompletedTask;
            //TODO
        }

        public void ShowAppointment(int calendarId, int appointmentId, int recurrenceIndex)
        {
            var (fragment, tag) = AppointmentFragment.NewInstance(calendarId, appointmentId, recurrenceIndex);

            fragmentManager.BeginTransaction()
               .SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out)
               .Replace(Resource.Id.fragment_container, fragment, tag)
               .AddToBackStack(tag)
               .Commit();
        }

        public void DeleteAppointmentsWithIds(List<int> appointmentIds)
        {
            uiCache.DeleteAppointmentsWithIds(appointmentIds);
        }

        #endregion

        #region ICalendarCoordinator implementation

        public bool DateDoubleTapped(Calendar calendar)
        {
            var (fragment, tag) = WeekCalendarFragment.NewInstance(calendar);

            fragmentManager.BeginTransaction()
               .SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out)
               .Replace(Resource.Id.fragment_container, fragment, tag)
               .AddToBackStack(tag)
               .Commit();

            return true;
        }

        public void YearTapped(Calendar calendar)
        {
            var (fragment, tag) = YearCalendarFragment.NewInstance(calendar);

            fragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
        }

        public void MonthTapped(Calendar calendar)
        {
            activity.OnBackPressed();
            monthCalendarFragment.MoveToDate(calendar);
        }

        public void CalendarsClicked()
        {
            presenter.ShowCalendarsListClicked();
        }

        public void CreateAppointmentClicked()
        {
            var (fragment, tag) = CreateAppointmentFragment.NewInstance();
            fragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
        }

        public void VisibleDatesChanged(Calendar startDate, Calendar endDate)
        {
            var start = startDate.ConvertToDateTime().ToLocalTime();
            var end = endDate.ConvertToDateTime().ToLocalTime();

            presenter.LoadAppointments(start, end);
        }

        public void RefreshClicked(Calendar startDate, Calendar endDate)
        {
            var start = startDate.ConvertToDateTime().ToLocalTime();
            var end = endDate.ConvertToDateTime().ToLocalTime();

            presenter.RefreshClicked(start, end);
        }

        public void MonthViewLoaded()
        {
            presenter = new CalendarPresenter();
            presenter.AttachView(this);
            presenter.Start();

            var fab = activity.Fab;
            fab.Visibility = ViewStates.Gone;

            activity.SupportActionBar.SetTitle(Resource.String.calendar);
        }

        #endregion

        #region ICalendarListCoordinator implementation

        public void SelectedCalendarsChanged(Dictionary<CalendarViewModel, bool> selectedCalendars)
        {
            var selectedCalendarsState = selectedCalendars.ToDictionary(k => k.Key.Id, k => k.Value);
            presenter.CalendarSelectionChanged(selectedCalendarsState);

            activity.OnBackPressed();
        }

        public void AppointmentTapped(ScheduleAppointment appointment)
        {
            var splitted = appointment.Notes.ToString().Split(" ");
            var calendarId = int.Parse(splitted[0]);
            var appointmentId = int.Parse(splitted[1]);
            var recurrenceIndex = int.Parse(splitted[2]);
            presenter.AppointmentClicked(calendarId, appointmentId, recurrenceIndex);
        }

        #endregion

        class UICache
        {
            public ObservableCollection<Appointment> Items { get; } = new ObservableCollection<Appointment>();

            List<AppointmentPreviewViewModel> AppointmentViewModels { get; } = new List<AppointmentPreviewViewModel>();
            List<CalendarViewModel> CalendarViewModels { get; } = new List<CalendarViewModel>();


            public void CacheAppointments(IEnumerable<AppointmentPreviewViewModel> appointmentViewModels, DateTime start, DateTime end)
            {
                CommonConfig.Logger.Debug($"PERIOD : {start} - {end}");
                AppointmentViewModels.Where(i => AppointmentIsInPeriod(i, start, end)).ToList().ForEach((obj) => AppointmentViewModels.Remove(obj));
                AppointmentViewModels.AddRange(appointmentViewModels);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Items.Where(i => AppointmentIsInPeriod(i, start, end)).ToList().ForEach((obj) => Items.Remove(obj));
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

                MainThread.BeginInvokeOnMainThread(() =>
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
                var appStart = appointment.Start.ConvertToDateTime();
                var appEnd = appointment.End.ConvertToDateTime();

                return DateTimeInPeriod(appStart, appEnd, start, end);
            }

            (int calendarId, int id, int recurrenceIndex) GetAppointmentInfo(Appointment appointment)
            {
                var splitted = appointment.Id.Split(" ");
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
                    Subject = cavm.Subject,
                    AllDay = cavm.AllDay,
                    Start = cavm.Start.ConvertToCalendar(),
                    End = cavm.End.ConvertToCalendar(),
                    Color = Android.Graphics.Color.ParseColor(cavm.HexColor),
                    Id = $"{cavm.CalendarId} {cavm.Id} {cavm.RecurrenceIndex}",
                };
            }

            #endregion
        }
    }

    public class Appointment
    {
        public int CalendarId { get; set; }
        public bool AllDay { get; set; }
        public string Id { get; set; }
        public string Subject { get; set; }
        public Calendar Start { get; set; }
        public Calendar End { get; set; }
        public int Color { get; set; }
    }

    public interface ICalendarCoordinator
    {
        ObservableCollection<Appointment> Items { get; }

        void MonthViewLoaded();

        bool DateDoubleTapped(Calendar calendar);
        void CalendarsClicked();
        void CreateAppointmentClicked();
        void VisibleDatesChanged(Calendar startDate, Calendar endDate);
        void AppointmentTapped(ScheduleAppointment appointment);
        void SelectedCalendarsChanged(Dictionary<CalendarViewModel, bool> selectedCalendars);
        void YearTapped(Calendar calendar);
        void MonthTapped(Calendar newValue);
        void RefreshClicked(Calendar startDate, Calendar endDate);
    }

}
