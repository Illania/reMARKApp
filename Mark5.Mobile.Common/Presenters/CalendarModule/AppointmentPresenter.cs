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
        int recurrenceIndex;

        public override void Start() { }

        public override void Stop() { }

        public async Task LoadAppointment(int appointmentId, int recurrenceIndex, int calendarId)
        {
            view.ShowLoading();

            try
            {
                CommonConfig.Logger.Info($"Retrieving appointment: AppointmentId = {appointmentId}, RecurrenceIndex = {recurrenceIndex}, CalendarId = {calendarId} ");

                appointment = await Managers.CalendarManager.GetCalendarAppointmentAsync(calendarId, appointmentId, SourceType.Local);
                this.recurrenceIndex = recurrenceIndex;
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
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool AllDay { get; set; }
        public string Creator { get; set; }
        public string RecurrenceInfo { get; set; }
        public long ReminderTimeBefore { get; set; }
        public List<ParticipantsViewModel> Participants { get; set; }


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

            string pattern = string.Empty;
            string range = string.Empty;

            switch (ri.Type)
            {
                case RecurrenceType.Daily:
                    pattern = "Daily, every ";

                    if (ri.WeekDays == WeekDays.EveryDay)
                        pattern += $"{ri.Periodicity} day(s)";
                    else //ri.WeekDays == WeekDays.WorkDays
                        pattern += $"weekday";
                    break;
                case RecurrenceType.Weekly:
                    {
                        pattern = $"Weekly, every {ri.Periodicity} week(s) on";

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
                    }

                case RecurrenceType.Monthly:
                    pattern = $"Monthly, ";
                    if (ri.WeekOfMonth == WeekOfMonth.None)
                        pattern += $"on day {ri.DayNumber} of every {ri.Periodicity} months";
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
                    range = $"ends after {ri.OccurrenceCount} occurrences";
                    break;
                case RecurrenceRange.EndByDate:
                    range = $"ends by {ri.EndDate.ToString("d", CultureInfo.CurrentCulture)}";
                    break;
            }

            if (!string.IsNullOrEmpty(range))
                range += ", ";

            range += $"starting from {ri.StartDate.ToString("d", CultureInfo.CurrentCulture)}";

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
                default: return string.Empty; //TODO always valid?
            }
        }

        public static string GetMonthString(int val)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(val);  //We shouldn't get the current culture I think
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
