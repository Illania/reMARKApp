using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Com.Syncfusion.Schedule;
using Com.Syncfusion.Schedule.Enums;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class MonthCalendarFragment : BaseCalendarFragment, IMenuItemOnMenuItemClickListener
    {
        public static (MonthCalendarFragment fragment, string tag) NewInstance()
        {
            var fragment = new MonthCalendarFragment();

            var tag = $"{nameof(MonthCalendarFragment)}";

            var args = new Bundle();
            fragment.Arguments = args;

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;

            (Activity as BaseAppCompatActivity).Fab.Visibility = ViewStates.Gone;

            if (schedule == null)
            {
                schedule = new MonthSchedule(Context);
                schedule.CellDoubleTapped += Schedule_CellDoubleTapped;
                schedule.HeaderTapped += Schedule_HeaderTapped;
                schedule.MonthInlineAppointmentTapped += Schedule_MonthInlineAppointmentTapped;
            }

            return schedule;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var refresh = menu.Add(Menu.None, MenuItemActions.Refresh, MenuItemActions.Refresh, Resource.String.refresh);
            refresh.SetIcon(Resource.Drawable.refresh);
            refresh.SetShowAsAction(ShowAsAction.Always);
            refresh.SetOnMenuItemClickListener(this);

            var calendarSelection = menu.Add(Menu.None, MenuItemActions.CalendarSelection, MenuItemActions.CalendarSelection, Resource.String.calendars);
            calendarSelection.SetIcon(Resource.Drawable.calendar);
            calendarSelection.SetShowAsAction(ShowAsAction.Always);
            calendarSelection.SetOnMenuItemClickListener(this);

            var addAppointment = menu.Add(Menu.None, MenuItemActions.CreateAppointment, MenuItemActions.CreateAppointment, Resource.String.add_appointment);
            addAppointment.SetIcon(Resource.Drawable.add_appointment);
            addAppointment.SetShowAsAction(ShowAsAction.Always);
            addAppointment.SetOnMenuItemClickListener(this);
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.Refresh)
                Refresh();

            if (item.ItemId == MenuItemActions.CalendarSelection)
                coordinator.CalendarsClicked();

            if (item.ItemId == MenuItemActions.CreateAppointment)
                coordinator.CreateAppointmentClicked();

            return true;
        }

        public override void OnResume()
        {
            base.OnResume();

            (Activity as AppCompatActivity).SupportActionBar.SetTitle(Resource.String.calendar);
        }

        void Refresh()
        {
            var startDate = schedule.VisibleDates.First();
            var endDate = schedule.VisibleDates.Last();

            coordinator.RefreshClicked(startDate, endDate);
        }
        void Schedule_MonthInlineAppointmentTapped(object sender, MonthInlineAppointmentTappedEventArgs e)
        {
            coordinator.AppointmentTapped(e.ScheduleAppointment);
        }

        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            coordinator.DateDoubleTapped(e.Calendar);
        }

        private void Schedule_HeaderTapped(object sender, HeaderTappedEventArgs e)
        {
            coordinator.YearTapped(e.Calendar);
        }

        static class MenuItemActions
        {
            public const int Refresh = 10;
            public const int CalendarSelection = 20;
            public const int CreateAppointment = 30;
        }
    }

    public class MonthSchedule : SfSchedule
    {
        readonly int darkerBlueColor;
        readonly int whiteColor;
        readonly int lightBlueColor;

        public MonthSchedule(Context context) : base(context)
        {
            darkerBlueColor = ContextCompat.GetColor(context, Resource.Color.darkerblue);
            whiteColor = ContextCompat.GetColor(context, Resource.Color.white);
            lightBlueColor = ContextCompat.GetColor(context, Resource.Color.lightblue);

            ViewHeaderStyle dayHeaderStyle = new ViewHeaderStyle
            {
                BackgroundColor = whiteColor,
                DayTextColor = darkerBlueColor,
            };

            HeaderStyle headerStyle = new HeaderStyle
            {
                BackgroundColor = whiteColor,
                TextColor = darkerBlueColor,
            };

            ScheduleView = ScheduleView.MonthView;
            HeaderStyle = headerStyle;
            ViewHeaderStyle = dayHeaderStyle;
            MonthViewSettings = new MonthViewSettings
            {
                ShowAgendaView = true,
                TodayBackgroundColor = new Color(lightBlueColor),
                SelectionTextColor = new Color(darkerBlueColor)
            };

            MonthCellLoaded += MonthSchedule_MonthCellLoaded;
        }


        void MonthSchedule_MonthCellLoaded(object sender, MonthCellLoadedEventArgs e)
        {
            CellStyle cellStyle;

            if (e.IsToday)
            {
                cellStyle = new CellStyle
                {
                    BackgroundColor = whiteColor,
                    TextColor = darkerBlueColor,
                };
            }
            else
            {
                cellStyle = new CellStyle
                {
                    BackgroundColor = whiteColor,
                    TextColor = darkerBlueColor,
                };
            }

            e.CellStyle = cellStyle;
        }

    }
}
