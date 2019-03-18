using System.Collections.Generic;

namespace Mark5.Mobile.Common.Presenters.CalendarModule
{
    public class CalendarPresenter : BasePresenter<ICalendarView>, ICalendarPresenter
    {
        public void ClickAppointment()
        {
            throw new System.NotImplementedException();
        }

        public override void Start()
        {
            throw new System.NotImplementedException();
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }
    }

    public class CalendarAppointmentViewModel
    {

    }

    public interface ICalendarPresenter : IPresenter<ICalendarView>
    {
        void ClickAppointment();

    }

    public interface ICalendarView : IView
    {
        void ShowLoading();
        void StopLoading();
        void ShowError();
        void UpdateAppointments(IEnumerable<CalendarAppointmentViewModel> caViewModels);
    }
}
