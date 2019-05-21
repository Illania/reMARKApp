using System;
using Android.OS;
using Android.Views;
using Android.Widget;
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
        private bool viewModeDay;
        private WeekCalendarConfiguration calendarConfiguration;
        private ICalendarActivity iCalendarActivity;

        public static (WeekCalendarFragment fragment, string tag) NewInstance()
        {
            var args = new Bundle();

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
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;
            calendarConfiguration = new WeekCalendarConfiguration();
            return calendarConfiguration.GetContent(Context);
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            iCalendarActivity = (CalendarActivity)context;
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
                if (calendarConfiguration != null)
                {
                    calendarConfiguration.ChangeViewMode();
                    viewModeDay = !viewModeDay;
                }
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
    }

    public class WeekCalendarConfiguration : IDisposable
    {
        private FrameLayout mainView;
        private Context context;
        private SfSchedule schedule;

        public View GetContent(Context con)
        {
            context = con;

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

            schedule = new SfSchedule(context)
            {
                ScheduleView = ScheduleView.WorkWeekView,
                MonthViewSettings = new MonthViewSettings()
                {
                    ShowAgendaView = true,
                    TodayBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.lightblue)),
                    SelectionTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.white))
                }
            };

            schedule.HeaderStyle = headerStyle;
            schedule.ViewHeaderStyle = dayHeaderStyle;

            mainView = new FrameLayout(con);
            mainView.AddView(schedule);

            return mainView;
        }

        public void ChangeViewMode()
        {
            schedule.ScheduleView = schedule.ScheduleView == ScheduleView.WorkWeekView ? ScheduleView.DayView : ScheduleView.WorkWeekView;
        }

        public void Dispose()
        {
            //TODO : implement
        }
    }
}
