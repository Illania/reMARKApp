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

            view.ShowEditingLoading();

            try
            {
                CommonConfig.Logger.Info($"Adding or editing appointment: AppointmentId = {ca.Id}, CalendarId = {ca.CalendarId} ");

                await Managers.CalendarManager.CreateOrUpdateCalendarAppointmentAsync(ca.CalendarId, ca);

                view.StopEditingLoading();
                view.CloseView();

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while adding or editing appointment: AppointmentId = {ca.Id}, CalendarId = {ca.CalendarId} ", ex);

                view.StopEditingLoading();

                await view.ShowEditingError(ex);
            }
        }

        public Task LoadEmptyAppointment(DateTime startDate)
        {
            var preselectedCalendar = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.First(c => !c.Shared)
                ?? ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.First();

            view.ShowAppointment(new AddEditAppointmentViewModel(startDate, ServerConfig.SystemSettings.UserInfo.User.Id)
            {
                Calendar = CalendarViewModel.ConvertToViewModel(preselectedCalendar),
            });

            return Task.CompletedTask;
        }

        public async Task LoadAppointment(int calendarId, int appointmentId)
        {
            try
            {
                CommonConfig.Logger.Info($"Retrieving appointment: AppointmentId = {appointmentId}, CalendarId = {calendarId} ");

                var appointment = await Managers.CalendarManager.GetCalendarAppointmentAsync(calendarId, appointmentId, SourceType.Local);

                view.ShowAppointment(AddEditAppointmentViewModel.ConvertToViewModel(appointment));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting appointment with ID = {appointmentId}", ex);

                await view.ShowLoadError(ex);

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
        public RecurrenceInfo RecurrenceInfo { get; set; }
        public long ReminderTimeBeforeStart { get; set; }
        public List<ParticipantsViewModel> Participants { get; set; }
        public CalendarViewModel Calendar { get; set; }

        public AddEditAppointmentViewModel() { }

        // When creating a new appointment default values are initialized here
        public AddEditAppointmentViewModel(DateTime start, int creatorId)
        {
            Id = -1;
            CreatorId = creatorId;
            ReminderTimeBeforeStart = -1;
            Participants = new List<ParticipantsViewModel>();

            if (start != default)
                Start = start;
            else
                Start = DateTime.Now.RoundUp(TimeSpan.FromMinutes(15));

            End = Start.AddMinutes(30);
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
                Id = Id,
                CalendarId = Calendar.Id,
                Subject = Subject ?? string.Empty,
                Description = Description ?? string.Empty,
                Location = Location ?? string.Empty,
                AllDay = AllDay,
                ReminderTimeBeforeStart = ReminderTimeBeforeStart,
                RecurrenceInfo = RecurrenceInfo,
                Participants = Participants.Select(ParticipantsViewModel.ConvertToModel).ToList(),
                CreatorId = CreatorId,
                Type = RecurrenceInfo == null ? CalendarOccurenceType.Normal : CalendarOccurenceType.Pattern,
            };

            if (ReminderTimeBeforeStart >= 0)
                ca.ReminderAlertDate = Start.AddSeconds(-ReminderTimeBeforeStart);

            ca.Occurrences.Add(new CalendarAppointmentOccurrence
            {
                RecurrenceIndex = -1,
                StartDate = Start,
                EndDate = End,
            });

            return ca;
        }

        public RecurrenceInfo GetEmptyRecurrenceInfo()
        {
            var recInfo = new RecurrenceInfo
            {
                Type = RecurrenceType.Daily,
                DayNumber = 1,
                Month = 1,
                WeekDays = WeekDays.EveryDay,
                Periodicity = 1,
                WeekOfMonth = WeekOfMonth.First,
                OccurrenceCount = 1,
                FirstDayOfWeek = DayOfWeek.Monday,
                StartDate = new DateTime(Start.Year, Start.Month, Start.Day, 0, 0, 1, DateTimeKind.Local),
                Range = RecurrenceRange.NoEndDate,
            };

            recInfo.EndDate = recInfo.StartDate.AddDays(10);

            return recInfo;
        }
    }

    public interface IAddEditAppointmentPresenter : IPresenter<IAddEditAppointmentView>
    {
        Task LoadEmptyAppointment(DateTime startDate);
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
        Task ShowLoadError(Exception ex);

        void ShowEditingLoading();
        void StopEditingLoading();
        Task ShowEditingError(Exception ex);
    }
}
