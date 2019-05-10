using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Presenters.CalendarModule
{
    public class CalendarPresenter : BasePresenter<ICalendarView>, ICalendarPresenter
    {
        protected List<Calendar> calendarsList;
        Dictionary<int, bool> calendarsSelectedState = new Dictionary<int, bool>();
        IAppointmentsCache Cache => Managers.CalendarManager.AppointmentsCache;

        #region ICalendarPresenter

        public override void Start()
        {
            calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;
            calendarsList.ForEach(c => calendarsSelectedState.Add(c.Id, true));

            Cache.Start();
            Cache.AppointmentRetrieved += Cache_AppointmentRetrieved;  //TODO we need to have an event also for errors

            UpdateCalendarsInView();
        }

        public override void Stop()
        {
            Cache.Stop();
        }

        public void AppointmentClicked(int appointmentId)
        {
            view.ShowAppointment(appointmentId);
        }

        public void LoadAppointments(DateTime start, DateTime end)
        {
            //view.ShowLoading(); //TODO ??

            var selectedCalendars = calendarsList?.Select(c => c.Id).ToList();
            Cache?.GetAppointments(selectedCalendars, start, end);
        }

        public void CalendarSelectionChanged(Dictionary<int, bool> calendarsSelectedState)
        {
            this.calendarsSelectedState.Clear();
            foreach (var k in calendarsSelectedState)
                this.calendarsSelectedState.Add(k.Key, k.Value);

            UpdateCalendarsInView();
        }

        public void ShowCalendarsListClicked()
        {
            var sel = new Dictionary<CalendarViewModel, bool>();

            foreach (var cal in calendarsList)
            {
                sel.Add(CalendarViewModel.ConvertToViewModel(cal), calendarsSelectedState[cal.Id]); //TODO
            }

            view.ShowCalendarsList(sel);
        }

        #endregion

        #region Cache event handlers

        void Cache_AppointmentRetrieved(object sender, AppointmentsRetrievedEventArgs e)
        {
            var appointmentsViewModels = e.Appointments?.Select(AppointmentPreviewViewModel.ConvertToViewModel);
            view.UpdateAppointments(appointmentsViewModels, e.Start, e.End);
            view.StopLoading();
        }

        #endregion

        #region Utilities

        void UpdateCalendarsInView()
        {
            view.CalendarsSelected(calendarsList.Where(c => calendarsSelectedState[c.Id]).Select(CalendarViewModel.ConvertToViewModel).ToList());
        }

        #endregion
    }

    public class AppointmentPreviewViewModel
    {
        public int Id { get; set; }
        public int CalendarId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string Subject { get; set; }
        public bool AllDay { get; set; }
        public string HexColor { get; set; }

        public static AppointmentPreviewViewModel ConvertToViewModel(CalendarAppointment ca)
        {
            return new AppointmentPreviewViewModel
            {
                Id = ca.Id,
                CalendarId = ca.CalendarId,
                Subject = ca.Subject,
                AllDay = ca.AllDay,
                Start = ca.Occurrences[0].StartDate,
                End = ca.Occurrences[0].EndDate,
                HexColor = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.First(c => c.Id == ca.CalendarId).ColorHex, //TODO should be improved
            };
        }
    }

    public class CalendarViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HexColor { get; set; }
        public bool Shared { get; set; }

        public static CalendarViewModel ConvertToViewModel(Calendar ca)
        {
            return new CalendarViewModel
            {
                Id = ca.Id,
                Name = ca.Name,
                HexColor = ca.ColorHex,
                Shared = ca.Shared,
            };
        }
    }

    public interface ICalendarPresenter : IPresenter<ICalendarView>
    {
        void LoadAppointments(DateTime start, DateTime end);
        void AppointmentClicked(int appointmentId);
        void CalendarSelectionChanged(Dictionary<int, bool> calendarsSelectedState);
        void ShowCalendarsListClicked();
    }

    public interface ICalendarView : IView
    {
        void CalendarsSelected(List<CalendarViewModel> calendars);
        void ShowCalendarsList(Dictionary<CalendarViewModel, bool> calendars);

        void UpdateAppointments(IEnumerable<AppointmentPreviewViewModel> caViewModels, DateTime start, DateTime end);

        void ShowLoading();
        void StopLoading();
        Task ShowError(Exception ex);
        void ShowAppointment(int appointmentId);
    }
}
