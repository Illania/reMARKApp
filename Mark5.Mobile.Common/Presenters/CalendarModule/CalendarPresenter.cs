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

        public override void Start()
        {
            calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;
            calendarsList.ForEach(c => calendarsSelectedState.Add(c.Id, true));

            Cache.Start();
        }

        public override void Stop()
        {
            Cache.Stop();
        }

        public void AppointmentClicked(int appointmentId)
        {
            //TODO to complete...
        }

        public async Task LoadAppointments(DateTime start, DateTime end)
        {
            view.ShowLoading();

            //var selectedCalendars = calendarsSelectedState?.Where(c => calendarsSelectedState[c.Key]).Select(c => c.Key).ToList();  //TODO testing!!
            var selectedCalendars = calendarsSelectedState?.Select(c => c.Key).ToList();

            try
            {
                var appointments = await Cache?.GetAppointments(selectedCalendars, start, end);

                var appointmentsViewModels = appointments?.Select(SimpleCalendarAppointmentViewModel.ConvertToViewModel);

                view.UpdateAppointments(appointmentsViewModels);
                view.StopLoading();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting appointments " +
                    $"in calendars {string.Join(", ", selectedCalendars)} from {start} to {end} ", ex);

                view.StopLoading();
                await view.ShowError(ex);
            }
        }

        public void CalendarSelectionChanged(int calendarId, bool isSelected)
        {
            calendarsSelectedState[calendarId] = isSelected;

            if (isSelected)
            {
                //Need to get those new appointments and add them to the view
            }
            else
            {
                //Need to remove from the showed ones
            }
        }
    }

    public class SimpleCalendarAppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string Subject { get; set; }
        public bool AllDay { get; set; }
        public string HexColor { get; set; }

        public static SimpleCalendarAppointmentViewModel ConvertToViewModel(CalendarAppointment ca)
        {
            return new SimpleCalendarAppointmentViewModel()
            {
                Id = ca.Id,
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

    }

    public interface ICalendarPresenter : IPresenter<ICalendarView>
    {
        Task LoadAppointments(DateTime start, DateTime end);
        void AppointmentClicked(int appointmentId);
        void CalendarSelectionChanged(int calendarId, bool isSelected);
    }

    public interface ICalendarView : IView
    {
        void SetCalendars(List<CalendarViewModel> calendars);
        void UpdateAppointments(IEnumerable<SimpleCalendarAppointmentViewModel> caViewModels);

        void ShowLoading();
        void StopLoading();
        Task ShowError(Exception ex);
    }
}
