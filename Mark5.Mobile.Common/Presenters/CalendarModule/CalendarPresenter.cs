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
        Dictionary<int, bool> calendarsSelectedState = new Dictionary<int, bool>();

        public override void Start()
        {
            calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;
            calendarsList.ForEach(c => calendarsSelectedState.Add(c.Id, true)); //To be cached
        }

        public override void Stop() { }

        public void AppointmentClicked(int appointmentId)
        {
            throw new System.NotImplementedException();
        }

        public async Task LoadAppointments(DateTime start, DateTime end)
        {
            view.ShowLoading();

            var selectedCalendars = calendarsSelectedState?.Where(c => calendarsSelectedState[c.Key]).Select(c => c.Key);

            try
            {
                var appointments = await Managers.CalendarManager.GetCalendarAppointmentsAsync(selectedCalendars.ToList(), start, end);
                var appointmentsViewModels = appointments.Select(ConvertToViewModel);

                view.UpdateAppointments(appointmentsViewModels);

                view.StopLoading();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting appointments " +
                    $"in calendars {string.Join(", ", selectedCalendars)} from {start} to {end} ", ex);

                view.StopLoading();
                view.ShowError();
            }
        }

        public void CalendarSelectionChanged(int calendarId, bool isSelected)
        {
            calendarsSelectedState[calendarId] = isSelected;
        }

        CalendarAppointmentViewModel ConvertToViewModel(CalendarAppointment ca)
        {
            return new CalendarAppointmentViewModel();
        }
    }

    public class CalendarAppointmentViewModel
    {

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
        void ShowLoading();
        void StopLoading();
        void ShowError();
        void UpdateAppointments(IEnumerable<CalendarAppointmentViewModel> caViewModels);
    }
}
