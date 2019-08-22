using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Extensions;

namespace Mark5.Mobile.Common.Presenters.CalendarModule
{
    public class AddEditAppointmentPresenter : BasePresenter<IAddEditAppointmentView>, IAddEditAppointmentPresenter
    {
        public override void Start() { }

        public override void Stop() { }

        public async Task AddOrEditAppointment(AddEditAppointmentViewModel vm)
        {
            var ca = vm.ConvertToModel();

            view.ShowLoading();

            try
            {
                CommonConfig.Logger.Info($"Adding or editing appointment: AppointmentId = {ca.Id}, CalendarId = {ca.CalendarId} ");

                await Managers.CalendarManager.CreateOrUpdateCalendarAppointmentAsync(ca.CalendarId, ca);

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
            view.ShowAppointment(new AddEditAppointmentViewModel(ServerConfig.SystemSettings.UserInfo.User.Id));
            return Task.CompletedTask;
        }

        public async Task LoadAppointment(int calendarId, int appointmentId)
        {
            view.ShowLoading();

            try
            {
                CommonConfig.Logger.Info($"Retrieving appointment: AppointmentId = {appointmentId}, CalendarId = {calendarId} ");

                var appointment = await Managers.CalendarManager.GetCalendarAppointmentAsync(calendarId, appointmentId, SourceType.Local);
                view.ShowAppointment(AddEditAppointmentViewModel.ConvertToViewModel(appointment));

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

    public class AddEditAppointmentViewModel
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
        public long ReminderTimeBeforeStart { get; set; }
        public List<ParticipantsViewModel> Participants { get; set; }
        public CalendarViewModel Calendar { get; set; }

        public AddEditAppointmentViewModel()
        { }

        // When creating a new appointment default values are initialized here
        public AddEditAppointmentViewModel(int creatorId)
        {
            CreatorId = creatorId;
            Start = DateTime.Now.RoundUp(TimeSpan.FromMinutes(15));
            End = DateTime.Now.RoundUp(TimeSpan.FromMinutes(15)).AddHours(1);
            ReminderTimeBeforeStart = -1;
        }

        public static AddEditAppointmentViewModel ConvertToViewModel(CalendarAppointment appointment)
        {
            var appModel = new AddEditAppointmentViewModel
            {
                Id = appointment.Id,
                Subject = appointment.Subject,
                Description = appointment.Description,
                Location = appointment.Location,
                AllDay = appointment.AllDay,
                CreatorId = appointment.CreatorId,
                RecurrenceInfo = appointment.RecurrenceInfo,
                ReminderTimeBeforeStart = appointment.ReminderTimeBeforeStart,
                Participants = appointment.Participants.Select(ParticipantsViewModel.ConvertToViewModel).ToList(),
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
                ReminderTimeBeforeStart = this.ReminderTimeBeforeStart,
                RecurrenceInfo = this.RecurrenceInfo,
                Participants = this.Participants.Select(ParticipantsViewModel.ConvertToModel).ToList(),
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

    public interface IAddEditAppointmentPresenter : IPresenter<IAddEditAppointmentView>
    {
        Task LoadEmptyAppointment();
        Task LoadAppointment(int calendarId, int appointmentId);
        void LoadCalendarsList();
        Task AddOrEditAppointment(AddEditAppointmentViewModel vm);
    }

    public interface IAddEditAppointmentView : IView
    {
        void ShowAppointment(AddEditAppointmentViewModel vm);
        void UpdateCalendarsList(List<CalendarViewModel> calendars);

        void CloseView();
        void ShowLoading();
        void StopLoading();
        Task ShowLoadError();
        Task ShowAddingEditingError(Exception ex);
    }
}
