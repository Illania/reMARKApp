using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Presenters.CalendarModule
{
    public class AppointmentPresenter : BasePresenter<IAppointmentView>, IAppointmentPresenter
    {
        CalendarAppointment appointment;

        public override void Start() { }

        public override void Stop() { }

        public async Task LoadAppointment(int appointmentId, int recurrenceIndex, int calendarId)
        {
            view.ShowAppointmentLoadingDialog();

            try
            {
                CommonConfig.Logger.Info($"Retrieving appointment: AppointmentId = {appointmentId}, RecurrenceIndex = {recurrenceIndex}, CalendarId = {calendarId} ");

                appointment = await Managers.CalendarManager.GetCalendarAppointmentAsync(calendarId, appointmentId, SourceType.Local);

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

        public void EditAppointmentClicked()
        {
            view.OpenEditAppointment(appointment.CalendarId, appointment.Id);
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
                    //TODO check if this can be done in a more clever way....
                    var newApp = await Managers.CalendarManager.GetCalendarAppointmentAsync(appointment.CalendarId, appointment.Id, SourceType.Remote);
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
                RecurrenceInfo = GetRecurrenceString(appointment.RecurrenceInfo),
                ReminderTimeBefore = appointment.ReminderTimeBeforeStart,
                Participants = appointment.Participants.Select(ParticipantsViewModel.ConvertToViewModel).ToList(),
            };

            var calendar = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.First(ca => ca.Id == appointment.CalendarId);
            appModel.Calendar = CalendarViewModel.ConvertToViewModel(calendar);

            var recurrence = appointment.Occurrences.FirstOrDefault(r => r.RecurrenceIndex == recurrenceIndex);

            if (recurrence == null)
                throw new ArgumentException("Can't find occurrence with the given recurrence index");

            appModel.Start = recurrence.StartDate;
            appModel.End = recurrence.EndDate;

            return appModel;
        }

        public static string GetRecurrenceString(RecurrenceInfo ri)
        {
            if (ri == null)
                return null;

            string pattern = "Reoccurs ";
            string range = string.Empty;

            switch (ri.Type)
            {
                case RecurrenceType.Daily:
                    pattern += "daily, every ";

                    if (ri.WeekDays == WeekDays.EveryDay)
                        pattern += $"{ri.Periodicity} day(s)";
                    else //ri.WeekDays == WeekDays.WorkDays
                        pattern += $"weekday";
                    break;
                case RecurrenceType.Weekly:
                    pattern += $"weekly, every {ri.Periodicity} week(s) on ";

                    var days = new[] { WeekDays.Monday, WeekDays.Tuesday, WeekDays.Wednesday,
                    WeekDays.Thursday, WeekDays.Friday, WeekDays.Saturday, WeekDays.Sunday};

                    var stringDays = new List<string>();

                    foreach (var day in days)
                    {
                        if (ri.WeekDays.HasFlag(day))
                            stringDays.Add(GetDayName(day));
                    }

                    pattern += string.Join(", ", stringDays);
                    break;
                case RecurrenceType.Monthly:
                    pattern += $"monthly, ";
                    if (ri.WeekOfMonth == WeekOfMonth.None)
                    {
                        string monthPatter = ri.Periodicity == 1 ? "month" : $"{ ri.Periodicity} months";
                        pattern += $"on day {ri.DayNumber} of every {monthPatter}";
                    }
                    else
                        pattern += $"the {GetWeekString(ri.WeekOfMonth)} {GetDayName(ri.WeekDays)} of every {ri.Periodicity} month(s) ";
                    break;
                case RecurrenceType.Yearly:
                    pattern += $"Yearly, ";

                    if (ri.WeekOfMonth == WeekOfMonth.None)
                        pattern += $"every {GetMonthString(ri.Month)}, {ri.DayNumber}";
                    else
                        pattern += $"the {GetWeekString(ri.WeekOfMonth)} {GetDayName(ri.WeekDays)} of {GetMonthString(ri.Month)} ";
                    break;
            }

            switch (ri.Range)
            {
                case RecurrenceRange.NoEndDate:
                    range = string.Empty;
                    break;
                case RecurrenceRange.OccurrenceCount:
                    range = $", ends after {ri.OccurrenceCount} occurrences";
                    break;
                case RecurrenceRange.EndByDate:
                    range = $", ends by {ri.EndDate.ToString("d", CultureInfo.CurrentCulture)}";
                    break;
            }

            return pattern + range;
        }

        public static string GetDayName(WeekDays val)
        {
            switch (val)
            {
                case WeekDays.Monday: return "Monday";
                case WeekDays.Tuesday: return "Tuesday";
                case WeekDays.Wednesday: return "Wednesday";
                case WeekDays.Thursday: return "Thursday";
                case WeekDays.Friday: return "Friday";
                case WeekDays.Saturday: return "Saturday";
                case WeekDays.Sunday: return "Sunday";
                case WeekDays.WeekendDays: return "Weekend day";
                case WeekDays.EveryDay: return "Day";
                case WeekDays.WorkDays: return "Weekday";
                default: return string.Empty;
            }
        }

        public static string GetWeekString(WeekOfMonth val)
        {
            switch (val)
            {
                case WeekOfMonth.First: return "first";
                case WeekOfMonth.Second: return "second";
                case WeekOfMonth.Third: return "third";
                case WeekOfMonth.Fourth: return "fourth";
                case WeekOfMonth.Last: return "last";
                default: return string.Empty;
            }
        }

        public static string GetMonthString(int val)
        {
            return CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(val);
        }
    }

    public class ParticipantsViewModel
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
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

        void EditAppointmentClicked();
    }

    public interface IAppointmentView : IView
    {
        void ShowAppointment(AppointmentViewModel appointment);
        void SetLines(IEnumerable<LineViewModel> lines);
        void CloseView();
        void OpenEditAppointment(int calendarId, int appointmentId);

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
