using System;
namespace Mark5.Mobile.Common.Presenters.CalendarModule
{
    public class AddEditAppointmentPresenter : BasePresenter<IEditAppointmentView>, IEditAppointmentPresenter
    {
        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }

    public interface IEditAppointmentPresenter : IPresenter<IEditAppointmentView>
    {

    }

    public interface IEditAppointmentView : IView
    {

    }
}
