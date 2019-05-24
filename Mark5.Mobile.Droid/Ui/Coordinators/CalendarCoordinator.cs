using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Support.V4.App;
using Android.Views;
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
        BaseAppCompatActivity activity;
        FragmentManager fragmentManager;
        MonthCalendarFragment monthFragment;

        CalendarPresenter presenter;

        readonly UICache uiCache;

        public ObservableCollection<Appointment> Items => uiCache.Items;

        public CalendarModuleCoordinator(BaseAppCompatActivity a)
        {
            activity = a;
            fragmentManager = a.SupportFragmentManager;

            uiCache = new UICache(this);
        }

        public (BaseFragment, string) GetMainFragment()
        {
            string tag;
            (monthFragment, tag) = MonthCalendarFragment.NewInstance();
            return (monthFragment, tag);
        }

        #region ICalendarView implementation

        public void CalendarsSelected(List<CalendarViewModel> calendars)
        {
            uiCache.SetCalendars(calendars);
        }

        public void ShowCalendarsList(Dictionary<CalendarViewModel, bool> calendars)
        {
            //TODO
        }

        public void UpdateAppointments(IEnumerable<AppointmentPreviewViewModel> caViewModels, DateTime start, DateTime end)
        {
            uiCache.CacheAppointments(caViewModels, start, end);
        }

        public void ShowLoading()
        {
            //TODO
        }

        public void StopLoading()
        {
            //TODO
        }

        public Task ShowError(Exception ex)
        {
            return Task.CompletedTask;
            //TODO
        }

        public void ShowAppointment(int appointmentId, int recurrenceIndex)
        {
            //TODO
        }

        #endregion

        #region ICalendar implementation

        public bool CellDoubleTapped(Calendar calendar)
        {
            var (fragment, tag) = WeekCalendarFragment.NewInstance();

            fragmentManager.BeginTransaction()
               .SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out)
               .Replace(Resource.Id.fragment_container, fragment, tag)
               .AddToBackStack(tag)
               .Commit();

            return true;
        }

        public void OnClick(View v)
        {
            var (fragment, tag) = YearCalendarFragment.NewInstance();
            fragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
            //SupportActionBar.Hide();
        }

        public void ShowToolBar()
        {
            //SupportActionBar.Show();
        }

        public void ShowCalendarSelection()
        {
            var (fragment, tag) = CalendarListFragment.NewInstance();
            fragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
            //SupportActionBar.Hide();
        }

        public void ShowCreateAppointment()
        {
            var (fragment, tag) = CreateAppointmentFragment.NewInstance();
            fragmentManager.BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment, tag)
                .AddToBackStack(tag)
                .Commit();
            //SupportActionBar.Hide();
        }

        public void VisibleDatesChanged(Calendar startDate, Calendar endDate)
        {
            var start = startDate.ConvertToDateTime().ToLocalTime();
            var end = endDate.ConvertToDateTime().ToLocalTime();

            presenter.LoadAppointments(start, end);
        }

        public void MonthViewLoaded()
        {
            presenter = new CalendarPresenter();
            presenter.AttachView(this);
            presenter.Start();
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
                    Id = $"{cavm.Id} {cavm.RecurrenceIndex}",
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
        public Java.Util.Calendar Start { get; set; }
        public Java.Util.Calendar End { get; set; }
        public int Color { get; set; }
    }

    public interface ICalendarCoordinator
    {
        ObservableCollection<Appointment> Items { get; }

        void MonthViewLoaded();
        bool CellDoubleTapped(Calendar calendar);

        void ShowCalendarSelection();

        void ShowCreateAppointment();
        void VisibleDatesChanged(Calendar startDate, Calendar endDate);
    }

}
