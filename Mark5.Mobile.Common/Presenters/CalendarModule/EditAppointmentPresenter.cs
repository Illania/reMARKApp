using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Presenters.CalendarModule
{
    public class AddEditAppointmentPresenter : BasePresenter<IEditAppointmentView>, IEditAppointmentPresenter
    {
        public override void Start() { }

        public override void Stop() { }

        public Task AddEditAppointment(EditAppointmentViewModel vm)
        {
            var appointment = new CalendarAppointment()
            {
                CalendarId = vm.c
            }





            return Task.CompletedTask;
        }

        public Task InitEmptyAppointment()
        {
            view.ShowAppointment(new EditAppointmentViewModel());
            return Task.CompletedTask;
        }

        public async Task LoadAppointment(int calendarId, int appointmentId, int recurrenceIndex)
        {
            view.ShowLoading();

            try
            {
                CommonConfig.Logger.Info($"Retrieving appointment: AppointmentId = {appointmentId}, RecurrenceIndex = {recurrenceIndex}, CalendarId = {calendarId} ");

                var appointment = await Managers.CalendarManager.GetCalendarAppointmentAsync(calendarId, appointmentId, SourceType.Local);
                view.ShowAppointment(EditAppointmentViewModel.ConvertToViewModel(appointment, recurrenceIndex));

                view.StopLoading();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting appointment with ID = {appointmentId}", ex);

                view.StopLoading();
                await view.ShowLoadError();

                view.CloseView();
            }
        }

    }

    public class EditAppointmentViewModel
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime RecurrenceStart { get; set; }
        public DateTime RecurrenceEnd { get; set; }
        public bool AllDay { get; set; }
        public string Creator { get; set; }
        public RecurrenceInfo RecurrenceInfo { get; set; } //TODO not so nice (use of domain class)
        public long ReminderTimeBefore { get; set; }
        public List<Participant> Participants { get; set; }  //TODO as before
        public int RecurrenceIndex { get; set; }
        public CalendarViewModel Calendar { get; set; }

        public static EditAppointmentViewModel ConvertToViewModel(CalendarAppointment appointment, int recurrenceIndex = -1)
        {
            var appModel = new EditAppointmentViewModel
            {
                Id = appointment.Id,
                Subject = appointment.Subject,
                Description = appointment.Description,
                Location = appointment.Location,
                AllDay = appointment.AllDay,
                Creator = appointment.Creator,
                RecurrenceIndex = recurrenceIndex,
                RecurrenceInfo = appointment.RecurrenceInfo,
                ReminderTimeBefore = appointment.ReminderTimeBeforeStart,
                Participants = appointment.Participants.ToList(),
            };

            var calendar = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.First(ca => ca.Id == appointment.CalendarId);
            appModel.Calendar = CalendarViewModel.ConvertToViewModel(calendar);

            var recurrence = appointment.Occurrences.FirstOrDefault(r => r.RecurrenceIndex == -1);

            if (recurrence == null)
                throw new ArgumentException("Can't find occurrence of the main appointment");

            appModel.Start = recurrence.StartDate;
            appModel.End = recurrence.EndDate;

            recurrence = appointment.Occurrences.FirstOrDefault(r => r.RecurrenceIndex == recurrenceIndex);

            if (recurrence == null)
                throw new ArgumentException("Can't find occurrence with the given recurrence index");

            appModel.RecurrenceStart = recurrence.StartDate;
            appModel.RecurrenceEnd = recurrence.EndDate;

            return appModel;
        }
    }

    public interface IEditAppointmentPresenter : IPresenter<IEditAppointmentView>
    {
        Task InitEmptyAppointment();
        Task LoadAppointment(int calendarId, int appointmentId, int recurrenceIndex);
        Task AddEditAppointment(EditAppointmentViewModel vm);
    }

    public interface IEditAppointmentView : IView
    {
        void ShowAppointment(EditAppointmentViewModel vm);

        void CloseView();
        Task ShowLoadError();
        void ShowLoading();
        void StopLoading();
    }
}
