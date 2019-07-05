using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Presenters.CalendarModule
{
    public class AppointmentPresenter : BasePresenter<IAppointmentView>, IAppointmentPresenter
    {
        private CalendarAppointment appointment;

        public override void Start() { }

        public override void Stop() { }

        public async Task LoadAppointment(int appointmentId, int recurrenceIndex, int calendarId)
        {
            view.ShowLoading();

            try
            {
                CommonConfig.Logger.Info($"Retrieving appointment with ID = {appointmentId}");

                appointment = await Managers.CalendarManager.GetCalendarAppointmentAsync(calendarId, appointmentId);
                view.ShowAppointment(AppointmentViewModel.ConvertToViewModel(appointment, recurrenceIndex));
                view.SetLines(ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Select(LineViewModel.ConvertToViewModel));

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

        public async Task DeleteAppointmentClicked()
        {
            view.ShowLoading();

            try
            {
                CommonConfig.Logger.Info($"Deleting appointment with ID = {appointment.Id}");

                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity> { appointment });

                view.StopLoading();
                view.CloseView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while deleting appointment with ID = {appointment.Id}", ex);

                view.StopLoading();
                await view.ShowDeleteError();
            }
        }

        public Task EditAppointmentClicked()
        {
            throw new NotImplementedException();
        }

        public async Task SendInvitationClicked(Guid lineGuid)
        {
            view.ShowLoading();

            try
            {
                CommonConfig.Logger.Info($"Sending invitations for appointment with ID = {appointment.Id}");

                await Managers.CalendarManager.SendCalendarAppointmentInvitationsAsync(appointment.Id, lineGuid);

                view.StopLoading();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while sending invitations for appointment with ID = {appointment.Id} with line with GUID = {lineGuid}", ex);

                view.StopLoading();
                await view.ShowSendInvitationError();
            }
        }
    }

    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public bool AllDay { get; set; }
        public string Creator { get; set; }
        public RecurrenceInfoViewModel RecurrenceInfo { get; set; }
        public long ReminderTimeBefore { get; set; }
        public List<ParticipantsViewModel> Participants { get; set; }


        public static AppointmentViewModel ConvertToViewModel(CalendarAppointment appointment, int recurrenceIndex = -1)
        {
            return new AppointmentViewModel
            {
                Id = appointment.Id,
                Subject = appointment.Subject,
                Description = appointment.Description,
                Location = appointment.Location,
                AllDay = appointment.AllDay,
                Creator = appointment.Creator,
                RecurrenceInfo = RecurrenceInfoViewModel.ConvertToViewModel(appointment.RecurrenceInfo),
                ReminderTimeBefore = appointment.ReminderTimeBeforeStart,
                Participants = appointment.Participants.Select(ParticipantsViewModel.ConvertToViewModel).ToList(),
            };
        }
    }

    public class RecurrenceInfoViewModel
    {
        public static RecurrenceInfoViewModel ConvertToViewModel(RecurrenceInfo ri)
        {
            return new RecurrenceInfoViewModel()  //TODO
            {
            };
        }
    }

    public class ParticipantsViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public ParticipantStatus Status { get; set; }

        public static ParticipantsViewModel ConvertToViewModel(Participant participant)
        {
            return new ParticipantsViewModel
            {
                Name = participant.CN,
                Email = participant.Email,
                Status = participant.Status,
            };
        }
    }

    public class LineViewModel
    {
        public string Name { get; set; }

        public static LineViewModel ConvertToViewModel(Line line)
        {
            return new LineViewModel
            {
                Name = line.Name
            };
        }
    }

    public interface IAppointmentPresenter : IPresenter<IAppointmentView>
    {
        Task LoadAppointment(int appointmentId, int recurrenceIndex, int calendarId);
        Task DeleteAppointmentClicked();
        Task EditAppointmentClicked();
        Task SendInvitationClicked(Guid lineGuid);
    }

    public interface IAppointmentView : IView
    {
        void ShowAppointment(AppointmentViewModel appointment);
        void SetLines(IEnumerable<LineViewModel> lines);
        void ShowLoading();
        void StopLoading();
        void CloseView();

        Task ShowLoadError();
        Task ShowDeleteError();
        Task ShowSendInvitationError();

    }
}
