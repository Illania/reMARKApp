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
    public class MonthCalendarFragment : BaseFragment, IMenuItemOnMenuItemClickListener
    {
        ICalendarActivity iCalendarActivity;
        MonthSchedule schedule;

        public static (MonthCalendarFragment fragment, string tag) NewInstance()
        {
            var fragment = new MonthCalendarFragment();

            var tag = $"{nameof(MonthCalendarFragment)}";

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

            schedule = new MonthSchedule(Context);
            schedule.CellDoubleTapped += Schedule_CellDoubleTapped;  //TODO refactor

            return schedule;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var calendarSelection = menu.Add(Menu.None, MenuItemActions.CalendarSelection, MenuItemActions.CalendarSelection, "Calendars");
            calendarSelection.SetTitle("Calendars");
            calendarSelection.SetShowAsAction(ShowAsAction.Always);
            calendarSelection.SetOnMenuItemClickListener(this);

            var insertTemplateItem = menu.Add(Menu.None, MenuItemActions.CreateAppointment, MenuItemActions.CreateAppointment, Resource.String.insert_template);
            insertTemplateItem.SetIcon(Resource.Drawable.action_add);
            insertTemplateItem.SetShowAsAction(ShowAsAction.Always);
            insertTemplateItem.SetOnMenuItemClickListener(this);
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.CalendarSelection)
                iCalendarActivity.ShowCalendarSelection();

            if (item.ItemId == MenuItemActions.CreateAppointment)
                iCalendarActivity.ShowCreateAppointment();

            return true;
        }

        static class MenuItemActions
        {
            public const int CreateAppointment = 11;
            public const int CalendarSelection = 10;
        }


        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            iCalendarActivity.CellDoubleTapped(); //TODO need to refactor...
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
