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

        public async Task LoadAppointment(int appointmentId, int calendarId)
        {
            view.ShowLoading();

            try
            {
                CommonConfig.Logger.Info($"Retrieving appointment with ID = {appointmentId}");

                appointment = await Managers.CalendarManager.GetCalendarAppointmentAsync(calendarId, appointmentId);
                view.ShowAppointment(AppointmentViewModel.ConvertToViewModel(appointment));
                view.SetLines(ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Select(LineViewModel.ConvertToViewModel));

                view.StopLoading();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting appointment with ID = {appointmentId}", ex);

                view.StopLoading();
                await view.ShowError();

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
                await view.ShowError();
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
                await view.ShowError();
            }
        }
    }

    public class AppointmentViewModel
    {
        public static AppointmentViewModel ConvertToViewModel(CalendarAppointment appointment)
        {
            return new AppointmentViewModel();
        }
    }

    public class LineViewModel
    {
        public static LineViewModel ConvertToViewModel(Line appointment)
        {
            return new LineViewModel();
        }
    }

    public interface IAppointmentPresenter : IPresenter<IAppointmentView>
    {
        Task LoadAppointment(int appointmentId, int calendarId);
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
        Task ShowError();
        void CloseView();
    }
}
