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

        public static (MonthCalendarFragment fragment, string tag) NewInstance()
        {
            var args = new Bundle();

            var fragment = new MonthCalendarFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(MonthCalendarFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            iCalendarActivity = (CalendarActivity)context;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;

            var configurator = new MonthCalendarConfiguration();
            configurator.DateSelected += Configurator_DateSelected; ;
            return configurator.GetContent(Context);
        }

        void Configurator_DateSelected()
        {
            if (iCalendarActivity != null)
                iCalendarActivity.CellDoubleTapped();
        }

        static class MenuItemActions
        {
            public const int CreateAppointment = 11;
            public const int CalendarSelection = 10;
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
            {
                iCalendarActivity.ShowCalendarSelection();
            }

            if (item.ItemId == MenuItemActions.CreateAppointment)
            {
                iCalendarActivity.ShowCreateAppointment();
            }
            return true;
        }
    }

    public class MonthCalendarConfiguration : IDisposable
    {
        private Context context;
        private FrameLayout mainView;
        public Action DateSelected;

        public View GetContent(Context con)
        {
            context = con;
            mainView = new FrameLayout(con);

            ViewHeaderStyle dayHeaderStyle = new ViewHeaderStyle
            {
                BackgroundColor = ContextCompat.GetColor(context, Resource.Color.darkerblue),
                DayTextColor = ContextCompat.GetColor(context, Resource.Color.white)
            };

            HeaderStyle headerStyle = new HeaderStyle
            {
                BackgroundColor = ContextCompat.GetColor(context, Resource.Color.darkerblue),
                TextColor = ContextCompat.GetColor(context, Resource.Color.white)
            };

            var schedule = new SfSchedule(context)
            {
                ScheduleView = ScheduleView.MonthView,
                MonthViewSettings = new MonthViewSettings()
                {
                    ShowAgendaView = true,
                    TodayBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.lightblue)),
                    SelectionTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.white))
                }
            };

            schedule.MonthCellLoaded += Schedule_MonthCellLoaded;
            schedule.CellDoubleTapped += Schedule_CellDoubleTapped;
            schedule.HeaderStyle = headerStyle;
            schedule.ViewHeaderStyle = dayHeaderStyle;

            mainView.AddView(schedule);

            return mainView;
        }

        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            if (DateSelected != null)
                DateSelected.Invoke();
        }

        void Schedule_MonthCellLoaded(object sender, MonthCellLoadedEventArgs e)
        {
            CellStyle cellStyle;

            if (e.IsToday)
            {
                cellStyle = new CellStyle
                {
                    BackgroundColor = ContextCompat.GetColor(context, Resource.Color.darkerblue),
                    TextColor = ContextCompat.GetColor(context, Resource.Color.darkerblue)
                };
            }
            else
            {
                cellStyle = new CellStyle
                {
                    BackgroundColor = ContextCompat.GetColor(context, Resource.Color.darkerblue),
                    TextColor = ContextCompat.GetColor(context, Resource.Color.white)
                };
            }

            e.CellStyle = cellStyle;
        }

        public void Dispose()
        {
            //TODO : implement
        }
    }
}

