using Android.OS;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Com.Syncfusion.Schedule;
using Com.Syncfusion.Schedule.Enums;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;
using System;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class WeekCalendarFragment : BaseCalendarFragment, IMenuItemOnMenuItemClickListener
    {
        const string InitialDateKey = "InitialDateKey";
        Java.Util.Calendar initialDate;

        bool viewModeDay;

        public static (WeekCalendarFragment fragment, string tag) NewInstance(Java.Util.Calendar initialDate)
        {
            var args = new Bundle();

            if (initialDate != null)
                args.PutString(InitialDateKey, Serializer.Serialize(initialDate.ConvertToDateTime()));

            var fragment = new WeekCalendarFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(WeekCalendarFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Arguments.ContainsKey(InitialDateKey))
                initialDate = Serializer.Deserialize<DateTime>(Arguments.GetString(InitialDateKey)).ConvertToCalendar();
        }

        protected override void SetSchedule()
        {
            schedule = new WeekSchedule(Context);
            schedule.CellTapped += Schedule_CellTapped;
            schedule.CellDoubleTapped += Schedule_CellDoubleTapped;
        }

        void Schedule_CellTapped(object sender, CellTappedEventArgs e)
        {
            if (e.ScheduleAppointment != null)
                coordinator.AppointmentTapped(e.ScheduleAppointment);
        }

        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            if (e.ScheduleAppointment != null)
                coordinator.AppointmentTapped(e.ScheduleAppointment);
            else
                coordinator.HourTapped(e.Calendar);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            schedule.MoveToDate = initialDate;
        }

        #region Toolbar setup

        static class MenuItemActions
        {
            public const int SwitchViewMode = 10;
            public const int Today = 20;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var switchViewModeItem = menu.Add(Menu.None, MenuItemActions.SwitchViewMode, MenuItemActions.SwitchViewMode, Resource.String.insert_template);
            switchViewModeItem.SetTitle(viewModeDay ? "Week" : "Day");
            switchViewModeItem.SetShowAsAction(ShowAsAction.Always);
            switchViewModeItem.SetOnMenuItemClickListener(this);

            var goToToday = menu.Add(Menu.None, MenuItemActions.Today, MenuItemActions.Today, Resource.String.today);
            goToToday.SetIcon(Resource.Drawable.today);
            goToToday.SetShowAsAction(ShowAsAction.Always);
            goToToday.SetOnMenuItemClickListener(this);
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.SwitchViewMode)
            {
                ChangeViewMode();
                viewModeDay = !viewModeDay;
                Activity.InvalidateOptionsMenu();
            }

            if (item.ItemId == MenuItemActions.Today)
                MoveToDate(Java.Util.Calendar.Instance);

            return true;
        }

        #endregion

        public void ChangeViewMode()
        {
            schedule.ScheduleView = schedule.ScheduleView == ScheduleView.WorkWeekView ? ScheduleView.DayView : ScheduleView.WorkWeekView;
        }

    }

    class WeekSchedule : SfSchedule
    {
        public WeekSchedule(Context context) : base(context)
        {
            ViewHeaderStyle dayHeaderStyle = new ViewHeaderStyle
            {
                BackgroundColor = ContextCompat.GetColor(context, Resource.Color.darkerblue),
                DayTextColor = ContextCompat.GetColor(context, Resource.Color.white),
                DateTextColor = ContextCompat.GetColor(context, Resource.Color.white),
                CurrentDayTextColor = ContextCompat.GetColor(context, Resource.Color.white),
                CurrentDateTextColor = ContextCompat.GetColor(context, Resource.Color.white)
            };

            HeaderStyle headerStyle = new HeaderStyle
            {
                BackgroundColor = ContextCompat.GetColor(context, Resource.Color.darkerblue),
                TextColor = ContextCompat.GetColor(context, Resource.Color.white)
            };

            ScheduleView = ScheduleView.WorkWeekView;
            MonthViewSettings = new MonthViewSettings
            {
                ShowAgendaView = true,
                TodayBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.lightblue)),
                SelectionTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.white))
            };

            HeaderStyle = headerStyle;
            ViewHeaderStyle = dayHeaderStyle;
            MinDisplayDate = new DateTime(2010, 1, 1).ConvertToCalendar();
        }
    }

}