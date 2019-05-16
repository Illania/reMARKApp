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
        List<Calendar> calendarsList;
        Dictionary<int, bool> calendarsSelectedState;
        Dictionary<int, string> calendarsColor;
        IAppointmentsCache Cache => Managers.CalendarManager.AppointmentsCache;

        bool firstLoad = true;

        #region ICalendarPresenter

        public CalendarPresenter()
        {
            calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;
            calendarsSelectedState = new Dictionary<int, bool>();
            calendarsColor = new Dictionary<int, string>();

            calendarsList.ForEach(c => calendarsColor.Add(c.Id, c.ColorHex));
        }

        public override void Start()
        {
            calendarsList.ForEach(c => calendarsSelectedState.Add(c.Id, true));

            Cache.AppointmentRetrieved += Cache_AppointmentRetrieved;  //TODO we need to have an event also for errors

            UpdateCalendarsInView();
        }

        public override void Stop()
        {
        }

        public void AppointmentClicked(int appointmentId)
        {
            view.ShowAppointment(appointmentId);
        }

        public void LoadAppointments(DateTime start, DateTime end)
        {
            if (firstLoad)
                view.ShowLoading();

            var selectedCalendars = calendarsList?.Select(c => c.Id).ToList();
            Cache?.GetAppointments(selectedCalendars, start, end);
        }

        public void CalendarSelectionChanged(Dictionary<int, bool> calendarsSelectedState)
        {
            if (this.calendarsSelectedState.Count == calendarsSelectedState.Count
                && calendarsSelectedState.All((arg) => this.calendarsSelectedState[arg.Key] == arg.Value))
                return;

            this.calendarsSelectedState.Clear();
            foreach (var k in calendarsSelectedState)
                this.calendarsSelectedState.Add(k.Key, k.Value);

            UpdateCalendarsInView();
        }

        public void ShowCalendarsListClicked()
        {
            var sel = new Dictionary<CalendarViewModel, bool>();

            foreach (var cal in calendarsList)
                sel.Add(CalendarViewModel.ConvertToViewModel(cal), calendarsSelectedState[cal.Id]);

            view.ShowCalendarsList(sel);
        }

        public void RefreshClicked(DateTime start, DateTime end)
        {
            Cache.Clean();

            firstLoad = true;

            LoadAppointments(start, end);
        }

        #endregion

        #region Cache event handlers

        void Cache_AppointmentRetrieved(object sender, AppointmentsRetrievedEventArgs e)
        {
            if (firstLoad)
                view.StopLoading();

            firstLoad = false;

            var appointmentsViewModels = e.Appointments?.Select(ConvertToViewModels).SelectMany(x => x);
            view.UpdateAppointments(appointmentsViewModels, e.Start, e.End);
            view.StopLoading();
        }

        List<AppointmentPreviewViewModel> ConvertToViewModels(CalendarAppointment ca)
        {
            return AppointmentPreviewViewModel.ConvertToViewModels(ca, calendarsColor);
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

        public static List<AppointmentPreviewViewModel> ConvertToViewModels(CalendarAppointment ca, Dictionary<int, string> calendarColors)
        {
            var appViewModels = new List<AppointmentPreviewViewModel>();
            if (ca.RecurrenceInfo == null)
            {
                appViewModels.Add(ConvertToViewModel(ca, ca.Occurrences[0], calendarColors));
            }
            else
            {
                foreach (var occ in ca.Occurrences)
                {
                    if (occ.RecurrenceIndex != -1)
                        appViewModels.Add(ConvertToViewModel(ca, occ, calendarColors));
                }
            }
            return appViewModels;
        }

        static AppointmentPreviewViewModel ConvertToViewModel(CalendarAppointment ca, CalendarAppointmentOccurrence cao, Dictionary<int, string> calendarColors)
        {
            return new AppointmentPreviewViewModel
            {
                Id = ca.Id,
                CalendarId = ca.CalendarId,
                Subject = ca.Subject,
                AllDay = ca.AllDay,
                Start = cao.StartDate,
                End = cao.EndDate,
                HexColor = calendarColors[ca.CalendarId],
            };
        }

        public override string ToString()
        {
            return string.Format("[AppViewModel: Id={0}, CalendarId={1}, Start={2}, End={3}, Subject={4}, AllDay={5}]", Id, CalendarId, Start, End, Subject, AllDay);
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
        void RefreshClicked(DateTime start, DateTime end);
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
