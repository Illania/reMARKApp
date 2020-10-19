using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using TinyMessenger;

namespace Mark5.Mobile.Common.Presenters.CalendarModule
{
    public class AppointmentPresenter : BasePresenter<IAppointmentView>, IAppointmentPresenter
    {
        CalendarAppointment appointment;
        TinyMessageSubscriptionToken editedAppointmentToken;

        public override void Start()
        {
            SubscribeToMessages();
        }

        public override void Stop()
        {
            UnsubscribeFromMessages();
        }

        public async Task LoadAppointment(int appointmentId, int recurrenceIndex, int calendarId)
        {
            try
            {
                CommonConfig.Logger.Info($"Retrieving appointment: AppointmentId = {appointmentId}, RecurrenceIndex = {recurrenceIndex}, CalendarId = {calendarId} ");

                view.ShowAppointmentLoadingDialog();
                appointment = await Managers.CalendarManager.GetCalendarAppointmentAsync(calendarId, appointmentId, recurrenceIndex, SourceType.Auto);

                view.ShowAppointment(AppointmentViewModel.ConvertToViewModel(appointment, recurrenceIndex));
                view.SetLines(ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Select(LineViewModel.ConvertToViewModel));

                view.CloseDialog();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting appointment with ID = {appointmentId}", ex);

                view.CloseDialog();
                await view.ShowLoadError(ex);

                view.CloseView();
            }
        }

        public async Task DeleteAppointmentClicked()
        {
            view.ShowDeletingDialog();

            try
            {
                CommonConfig.Logger.Info($"Deleting appointment with ID = {appointment.Id}");

                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity> { appointment });

                view.CloseDialog();
                view.CloseView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while deleting appointment with ID = {appointment.Id}", ex);

                view.CloseDialog();
                await view.ShowDeleteError(ex);
            }
        }

        public async void EditAppointmentClicked(AppointmentChangeType appointmentChangeType)
        {
            view.OpenEditAppointment(appointment.CalendarId, appointment.Id, appointmentChangeType);
        }

        public async Task SendInvitationsClicked(LineViewModel lvm)
        {
            view.ShowSendInvitationsDialog();

            try
            {
                CommonConfig.Logger.Info($"Sending invitations for appointment with ID = {appointment.Id}");

                await Managers.CalendarManager.SendCalendarAppointmentInvitationsAsync(appointment.Id, lvm.Guid);

                view.CloseDialog();

                try
                {
                    var newApp = await Managers.CalendarManager.GetCalendarAppointmentAsync(appointment.CalendarId, appointment.Id, -1, SourceType.Remote);
                    var participants = newApp.Participants.Select(ParticipantsViewModel.ConvertToViewModel).ToList();

                    view.UpdateParticipants(participants);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Error while updating participants for appointment with ID = {appointment.Id} with line with GUID = {lvm.Guid}", ex);

                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while sending invitations for appointment with ID = {appointment.Id} with line with GUID = {lvm.Guid}", ex);

                view.CloseDialog();
                await view.ShowSendInvitationError(ex);
            }

        }

        #region Messages handlers

        void SubscribeToMessages()
        {
            editedAppointmentToken = CommonConfig.MessengerHub.Subscribe<EntityChangedMessage>(HandleEditedAppointment, m => m.ObjectType == ObjectType.CalendarAppointment);
        }

        void UnsubscribeFromMessages()
        {
            editedAppointmentToken?.Dispose();
        }

        void HandleEditedAppointment(EntityChangedMessage obj)
        {
             view.CloseView();
        }

        #endregion
    }

    public class AppointmentViewModel
    {
        public int Id { get; private set; }
        public string Subject { get; private set; }
        public string Description { get; private set; }
        public string Location { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public bool AllDay { get; private set; }
        public string Creator { get; private set; }
        public string RecurrenceInfo { get; private set; }
        public long ReminderTimeBefore { get; private set; }
        public List<ParticipantsViewModel> Participants { get; private set; }
        public CalendarViewModel Calendar { get; private set; }
        public CalendarOccurenceType Type { get; set; }

        public static AppointmentViewModel ConvertToViewModel(CalendarAppointment appointment, int recurrenceIndex = -1)
        {
            var appModel = new AppointmentViewModel
            {
                Id = appointment.Id,
                Subject = appointment.Subject,
                Description = appointment.Description,
                Location = appointment.Location,
                AllDay = appointment.AllDay,
                Creator = appointment.Creator,
                RecurrenceInfo = appointment.RecurrenceInfo?.ToFriendlyString(),
                ReminderTimeBefore = appointment.ReminderTimeBeforeStart,
                Participants = appointment.Participants?.Select(ParticipantsViewModel.ConvertToViewModel).ToList(),
                Type = appointment.Type
            };

            var calendar = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.First(ca => ca.Id == appointment.CalendarId);
            appModel.Calendar = CalendarViewModel.ConvertToViewModel(calendar);
                    
            var occurrence = appointment.Occurrences.FirstOrDefault(r => r.RecurrenceIndex == recurrenceIndex);

            if (occurrence == null)
                throw new ArgumentException("Can't find occurrence with the given recurrence index");

            if (appModel.AllDay)
            {
                appModel.Start = occurrence.AllDayStartDate;
                appModel.End = occurrence.StartDate.Date == occurrence.EndDate.Date ? occurrence.AllDayEndDate : occurrence.AllDayEndDate.AddDays(-1);
                //https://documentation.devexpress.com/CoreLibraries/DevExpress.XtraScheduler.Appointment.AllDay.property
            }
            else
            {
                appModel.Start = occurrence.StartDate;
                appModel.End = occurrence.EndDate;
            }

            return appModel;
        }

    }

    public class ParticipantsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public ParticipantStatus Status { get; set; }
        public ParticipantType Type { get; set; }

        public static ParticipantsViewModel ConvertToViewModel(Participant participant)
        {
            return new ParticipantsViewModel
            {
                Id = participant.Id,
                Name = participant.CN,
                Email = participant.Email,
                Status = participant.Status,
                Type = participant.Type,
            };
        }

        public static Participant ConvertToModel(ParticipantsViewModel participant)
        {
            return new Participant
            {
                Id = participant.Id,
                CN = participant.Name,
                Email = participant.Email,
                Type = participant.Type,
                Status = participant.Status,
                Presence = ParticipantPresenence.Mandatory,
            };
        }
    }

    public class LineViewModel
    {
        public Guid Guid { get; private set; }
        public string Name { get; private set; }

        public static LineViewModel ConvertToViewModel(Line line)
        {
            return new LineViewModel
            {
                Guid = line.Guid,
                Name = line.Name
            };
        }
    }

    public interface IAppointmentPresenter : IPresenter<IAppointmentView>
    {
        Task LoadAppointment(int appointmentId, int recurrenceIndex, int calendarId);
        Task DeleteAppointmentClicked();
        Task SendInvitationsClicked(LineViewModel lvm);

        void EditAppointmentClicked(AppointmentChangeType appointmentChangeType);
    }

    public interface IAppointmentView : IView
    {
        void ShowAppointment(AppointmentViewModel appointment);
        void SetLines(IEnumerable<LineViewModel> lines);
        void CloseView();
        void OpenEditAppointment(int calendarId, int appointmentId, AppointmentChangeType appointmentChangeType);

        void ShowAppointmentLoadingDialog();
        void ShowDeletingDialog();
        void ShowSendInvitationsDialog();

        Task ShowLoadError(Exception ex);
        Task ShowDeleteError(Exception ex);
        Task ShowSendInvitationError(Exception ex);

        void CloseDialog();
        void UpdateParticipants(List<ParticipantsViewModel> participants);
    }
}
