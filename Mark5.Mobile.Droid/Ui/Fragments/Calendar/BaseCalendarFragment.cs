using Android.OS;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Coordinators;
using Com.Syncfusion.Schedule;
using System.Linq;
using Android.Graphics;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class BaseCalendarFragment : BaseFragment
    {
        const string SelectedDateKey = "SelectedDateKey";
        const string VisibleDateKey = "VisibleDateKey";

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

            View.SetBackgroundColor(Color.White);

            schedule.AppointmentMapping = GetAppointmentMapping();

            var visibleDate = (Java.Util.Calendar)savedInstanceState?.GetSerializable(VisibleDateKey);
            var selectedDate = (Java.Util.Calendar)savedInstanceState?.GetSerializable(SelectedDateKey);

            if (visibleDate != null)
                schedule.MoveToDate = visibleDate;

            if (selectedDate != null)
                schedule.SelectedDate = selectedDate;
        }

        public override void OnResume()
        {
            base.OnResume();

            schedule.VisibleDatesChanged += Schedule_VisibleDatesChanged;
            schedule.ItemsSource = coordinator.Items;
        }

        public override void OnPause()
        {
            base.OnPause();
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

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutSerializable(SelectedDateKey, schedule.SelectedDate);
            outState.PutSerializable(VisibleDateKey, schedule.VisibleDates.First());
        }

        public static AppointmentMapping GetAppointmentMapping()
        {
            AppointmentMapping mapping = new AppointmentMapping
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
