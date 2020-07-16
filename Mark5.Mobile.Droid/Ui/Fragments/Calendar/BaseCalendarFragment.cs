using Android.OS;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Coordinators;
using Com.Syncfusion.Schedule;
using System.Linq;
using Android.Graphics;
using Android.Support.Design.Widget;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public abstract class BaseCalendarFragment : BaseFragment
    {
        protected ICalendarCoordinator coordinator;
        protected CoordinatorLayout layout;
        protected SfSchedule schedule;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            coordinator = ((MainActivity)Activity).CalendarCoordinator;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;

            (Activity as BaseAppCompatActivity).Fab.Visibility = ViewStates.Gone;

            if (layout == null)
            {
                var fab = new FloatingActionButton(Context);
                var margin = (int)Resources.GetDimension(Resource.Dimension.fab_margin);
                var lparams = new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                lparams.Gravity = (int)GravityFlags.Bottom | (int)GravityFlags.Right;
                lparams.AnchorGravity = (int)GravityFlags.Bottom | (int)GravityFlags.Right;
                lparams.SetMargins(margin, margin, margin, margin);
                fab.LayoutParameters = lparams;
                fab.SetImageResource(Resource.Drawable.add_appointment);
                fab.Click += Fab_Click;

                layout = new CoordinatorLayout(Context);
                layout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

                SetSchedule();

                layout.AddView(schedule);
                layout.AddView(fab);
            }

            return layout;
        }

        protected abstract void SetSchedule();

        void Fab_Click(object sender, System.EventArgs e)
        {
            coordinator.CreateAppointmentClicked(schedule.SelectedDate);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            View.SetBackgroundColor(Color.White);

            schedule.AppointmentMapping = GetAppointmentMapping();
        }

        public override void OnResume()
        {
            base.OnResume();

            (Activity as BaseAppCompatActivity).Fab.Visibility = ViewStates.Gone;

            schedule.VisibleDatesChanged += Schedule_VisibleDatesChanged;
            UpdateSource();
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

        public void UpdateSource()
        {
            schedule.ItemsSource = coordinator.Items;
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
