using System;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Com.Syncfusion.Schedule;
using Com.Syncfusion.Schedule.Enums;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class MonthCalendarFragment : BaseCalendarFragment, IMenuItemOnMenuItemClickListener
    {
        public static (MonthCalendarFragment fragment, string tag) NewInstance()
        {
            var fragment = new MonthCalendarFragment();

            var tag = $"{nameof(MonthCalendarFragment)}";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;

            schedule = new MonthSchedule(Context);
            schedule.CellDoubleTapped += Schedule_CellDoubleTapped;  //TODO refactor

            ((AppCompatActivity)Activity).SupportActionBar.SetTitle(Resource.String.calendar);

            return schedule;
        }


        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var calendarSelection = menu.Add(Menu.None, MenuItemActions.CalendarSelection, MenuItemActions.CalendarSelection, "Calendars");
            calendarSelection.SetTitle("Calendars");
            calendarSelection.SetShowAsAction(ShowAsAction.Always);
            calendarSelection.SetOnMenuItemClickListener(this);

            var addAppointment = menu.Add(Menu.None, MenuItemActions.CreateAppointment, MenuItemActions.CreateAppointment, Resource.String.insert_template);  //TODO change
            addAppointment.SetIcon(Resource.Drawable.action_add);
            addAppointment.SetShowAsAction(ShowAsAction.Always);
            addAppointment.SetOnMenuItemClickListener(this);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            coordinator.MonthViewLoaded();
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.CalendarSelection)
                coordinator.CalendarsClicked();

            if (item.ItemId == MenuItemActions.CreateAppointment)
                coordinator.CreateAppointmentClicked();

            return true;
        }

        static class MenuItemActions
        {
            public const int CreateAppointment = 11;
            public const int CalendarSelection = 10;
        }


        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            coordinator.DateDoubleTapped(e.Calendar); //TODO need to refactor...
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
                BackgroundColor = darkerBlueColor,
                DayTextColor = whiteColor,
            };

            HeaderStyle headerStyle = new HeaderStyle
            {
                BackgroundColor = darkerBlueColor,
                TextColor = whiteColor,
            };

            ScheduleView = ScheduleView.MonthView;
            HeaderStyle = headerStyle;
            ViewHeaderStyle = dayHeaderStyle;
            MonthViewSettings = new MonthViewSettings
            {
                ShowAgendaView = true,
                TodayBackgroundColor = new Color(lightBlueColor),
                SelectionTextColor = new Color(whiteColor)
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
                    BackgroundColor = darkerBlueColor,
                    TextColor = darkerBlueColor,
                };
            }
            else
            {
                cellStyle = new CellStyle
                {
                    BackgroundColor = darkerBlueColor,
                    TextColor = whiteColor,
                };
            }

            e.CellStyle = cellStyle;
        }

    }
}
