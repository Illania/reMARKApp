using Android.OS;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Com.Syncfusion.Schedule;
using Com.Syncfusion.Schedule.Enums;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class WeekCalendarFragment : BaseFragment, IMenuItemOnMenuItemClickListener
    {
        bool viewModeDay;
        ICalendarActivity iCalendarActivity;
        WeekSchedule schedule;

        public static (WeekCalendarFragment fragment, string tag) NewInstance()
        {
            var fragment = new WeekCalendarFragment();
            var tag = $"{nameof(WeekCalendarFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            iCalendarActivity = ((MainActivity)Activity).CalendarCoordinator;

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;

            schedule = new WeekSchedule(Context);

            return schedule;
        }

        #region Toolbar setup
        static class MenuItemActions
        {
            public const int CreateAppoitnment = 11;
            public const int SwitchViewMode = 10;
            public const int SelectCalendars = 9;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var calendarSelectionItem = menu.Add(Menu.None, MenuItemActions.SelectCalendars, MenuItemActions.SelectCalendars, Resource.String.calendar);
            calendarSelectionItem.SetTitle("Calendars");
            calendarSelectionItem.SetShowAsAction(ShowAsAction.Always);
            calendarSelectionItem.SetOnMenuItemClickListener(this);

            var createAppointmentItem = menu.Add(Menu.None, MenuItemActions.CreateAppoitnment, MenuItemActions.CreateAppoitnment, Resource.String.insert_template);
            createAppointmentItem.SetIcon(Resource.Drawable.action_add);
            createAppointmentItem.SetShowAsAction(ShowAsAction.Always);
            createAppointmentItem.SetOnMenuItemClickListener(this);

            var switchViewModeItem = menu.Add(Menu.None, MenuItemActions.SwitchViewMode, MenuItemActions.SwitchViewMode, Resource.String.insert_template);
            switchViewModeItem.SetTitle(viewModeDay ? "Week" : "Day");
            switchViewModeItem.SetShowAsAction(ShowAsAction.Always);
            switchViewModeItem.SetOnMenuItemClickListener(this);
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.SwitchViewMode)
            {
                ChangeViewMode();
                viewModeDay = !viewModeDay;
                Activity.InvalidateOptionsMenu();
            }

            if (item.ItemId == MenuItemActions.SelectCalendars)
            {
                iCalendarActivity.ShowCalendarSelection();
            }

            if (item.ItemId == MenuItemActions.CreateAppoitnment)
            {
                iCalendarActivity.ShowCreateAppointment();
            }

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
        }
    }

}