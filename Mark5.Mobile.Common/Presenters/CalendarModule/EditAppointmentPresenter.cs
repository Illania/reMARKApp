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

        public async Task AddOrEditAppointment(EditAppointmentViewModel vm)
        {
            var ca = vm.ConvertToModel();

            view.ShowLoading();

            try
            {
                CommonConfig.Logger.Info($"Adding or editing appointment: AppointmentId = {ca.Id}, CalendarId = {ca.CalendarId} ");

                await Managers.CalendarManager.CreateOrUpdateCalendarAppointmentAsync(ca.CalendarId, ca);
                //TODO this should send message and update the db eventually

                view.StopLoading();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while adding or editing appointment: AppointmentId = {ca.Id}, CalendarId = {ca.CalendarId} ", ex);

                view.StopLoading();

                await view.ShowAddingEditingError(ex);

                view.CloseView();
            }
        }

        public Task LoadEmptyAppointment()
        {
            view.ShowAppointment(new EditAppointmentViewModel
            {
                CreatorId = ServerConfig.SystemSettings.UserInfo.User.Id,
            });
            return Task.CompletedTask;
        }

        public async Task LoadAppointment(int calendarId, int appointmentId)
        {
            view.ShowLoading();

            try
            {
                CommonConfig.Logger.Info($"Retrieving appointment: AppointmentId = {appointmentId}, CalendarId = {calendarId} ");

                var appointment = await Managers.CalendarManager.GetCalendarAppointmentAsync(calendarId, appointmentId, SourceType.Local);
                view.ShowAppointment(EditAppointmentViewModel.ConvertToViewModel(appointment));

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

        public void LoadCalendarsList()
        {
            var calendars = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.Select(CalendarViewModel.ConvertToViewModel);
            view.UpdateCalendarsList(calendars.ToList());
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
        public bool AllDay { get; set; }
        public int CreatorId { get; set; }
        public RecurrenceInfo RecurrenceInfo { get; set; } //TODO not so nice (use of domain class)
        public long ReminderTimeBefore { get; set; }
        public List<Participant> Participants { get; set; }  //TODO as before
        public CalendarViewModel Calendar { get; set; }

        public static EditAppointmentViewModel ConvertToViewModel(CalendarAppointment appointment)
        {
            var appModel = new EditAppointmentViewModel
            {
                Id = appointment.Id,
                Subject = appointment.Subject,
                Description = appointment.Description,
                Location = appointment.Location,
                AllDay = appointment.AllDay,
                CreatorId = appointment.CreatorId,
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

            return appModel;
        }

        public CalendarAppointment ConvertToModel()
        {
            var ca = new CalendarAppointment
            {
                Id = this.Calendar.Id,
                Subject = this.Subject,
                Description = this.Description,
                Location = this.Location,
                AllDay = this.AllDay,
                ReminderTimeBeforeStart = this.ReminderTimeBefore,
                RecurrenceInfo = this.RecurrenceInfo,
                Participants = this.Participants,
                CreatorId = this.CreatorId,
            };

            ca.Occurrences.Add(new CalendarAppointmentOccurrence
            {
                RecurrenceIndex = -1,
                StartDate = this.Start,
                EndDate = this.End,
            });

            return ca;
        }
    }

    public interface IEditAppointmentPresenter : IPresenter<IEditAppointmentView>
    {
        Task LoadEmptyAppointment();
        Task LoadAppointment(int calendarId, int appointmentId);
        void LoadCalendarsList();
        Task AddOrEditAppointment(EditAppointmentViewModel vm);
    }

    public interface IEditAppointmentView : IView
    {
        void ShowAppointment(EditAppointmentViewModel vm);
        void UpdateCalendarsList(List<CalendarViewModel> calendars);

        void CloseView();
        void ShowLoading();
        void StopLoading();
        Task ShowLoadError();
        Task ShowAddingEditingError(Exception ex);
    }
}
