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
                var appointmentsViewModels = appointments.Select(SimpleCalendarAppointmentViewModel.ConvertToViewModel);

                view.UpdateAppointments(appointmentsViewModels);

                view.StopLoading();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting appointments " +
                    $"in calendars {string.Join(", ", selectedCalendars)} from {start} to {end} ", ex);

                view.StopLoading();
                await view.ShowError();
            }
        }

        //TODO How do we implement our download/refresh policy....?
        /* Some ideas
         * 1) Get always appointments from all calendars, so we'll always have appointments for all calendars related to the same period      
         * 2) Get 1 year before and 1 year after the current date, so we do the donwnload once
         * 3) Get the current month, and then make several calls to get more months
         * 4) Preemptive download, so as soon as the user is less than 3-4 months away from the "edge" of what we have downloaded, we ask for more stuff
         * 
         * 
         */

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
        public static SimpleCalendarAppointmentViewModel ConvertToViewModel(CalendarAppointment ca)
        {
            return new SimpleCalendarAppointmentViewModel();
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
        Task ShowError();
    }
}
