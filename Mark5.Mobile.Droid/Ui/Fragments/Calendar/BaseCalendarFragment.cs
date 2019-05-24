using Android.OS;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Coordinators;
using Com.Syncfusion.Schedule;
using System.Linq;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class BaseCalendarFragment : BaseFragment
    {
        protected ICalendarCoordinator coordinator;
        protected SfSchedule schedule;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            coordinator = ((MainActivity)Activity).CalendarCoordinator;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            schedule.ItemsSource = coordinator.Items;
            schedule.AppointmentMapping = GetAppointmentMapping();
            schedule.VisibleDatesChanged += Schedule_VisibleDatesChanged;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            schedule.VisibleDatesChanged -= Schedule_VisibleDatesChanged;
        }

        private void Schedule_VisibleDatesChanged(object sender, VisibleDatesChangedEventArgs e)
        {
            var startDate = schedule.VisibleDates.First();
            var endDate = schedule.VisibleDates.Last();

            coordinator.VisibleDatesChanged(startDate, endDate);
        }

        public void MoveToDate(Java.Util.Calendar date)
        {
            if (schedule != null)
            {
                schedule.MoveToDate = date;
                schedule.SelectedDate = date;
            }
        }

        public static AppointmentMapping GetAppointmentMapping()
        {
            AppointmentMapping mapping = new AppointmentMapping //TODO need to check if all is correct
            {
                Subject = "Subject",
                StartTime = "Start",
                EndTime = "End",
                Color = "Color",
                Notes = "Id",
                IsAllDay = "AllDay"
            };
            return mapping;
        }

    }
}
