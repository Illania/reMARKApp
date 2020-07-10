using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Syncfusion.SfSchedule.iOS;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public abstract class CalendarViewController : AbstractViewController
    {
        protected SFSchedule schedule;
        protected ICalendarCoordinator Coordinator;

        protected CalendarViewController(ICalendarCoordinator coordinator)
        {
            Coordinator = coordinator;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            schedule.AppointmentMapping = GetAppointmentMapping();
            UpdateSource();
        }

        public void MoveToDate(NSDate date)
        {
            if (schedule != null)
            {
                schedule.MoveToDate(date);
                schedule.SelectedDate = date;
            }
        }

        public void UpdateSource()
        {
            schedule.ItemsSource = Coordinator.Items;
        }

        public static AppointmentMapping GetAppointmentMapping()
        {
            AppointmentMapping mapping = new AppointmentMapping
            {
                Subject = "Subject",
                StartTime = "Start",
                EndTime = "End",
                AppointmentBackground = "Color",
                Notes = "Id",
                IsAllDay = "AllDay"
            };
            return mapping;
        }
    }
}
