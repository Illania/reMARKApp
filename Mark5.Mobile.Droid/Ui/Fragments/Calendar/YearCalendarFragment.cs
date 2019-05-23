using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Views.Animations;
using Android.Support.V4.Content;
using Com.Syncfusion.Calendar;
using Com.Syncfusion.Calendar.Enums;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class YearCalendarFragment : BaseFragment
    {
        ICalendarActivity iCalendarActivity;
        YearCalendar yearCalendar;

        public static (YearCalendarFragment fragment, string tag) NewInstance()
        {
            var fragment = new YearCalendarFragment();
            var tag = $"{nameof(YearCalendarFragment)}";

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

            yearCalendar = new YearCalendar(Context);
            yearCalendar.MonthChanged += Calendar_MonthChanged;  //TODO need to move it 

            return yearCalendar;
        }

        void Configurator_MonthSelected()
        {
            iCalendarActivity.ShowToolBar();
            Activity.SupportFragmentManager.PopBackStack();
        }

        void Calendar_MonthChanged(object sender, MonthChangedEventArgs e)
        {
            Animation animation = AnimationUtils.LoadAnimation(Context, Resource.Animation.fade_out);
            yearCalendar.StartAnimation(animation);

            iCalendarActivity.ShowToolBar();
            Activity.SupportFragmentManager.PopBackStack();
        }
    }

    class YearCalendar : SfCalendar
    {
        public YearCalendar(Context context) : base(context)
        {
            MonthViewLabelSetting labelSettings = new MonthViewLabelSetting
            {
                DateLabelSize = 12
            };

            MonthViewSettings monthViewSettings = new MonthViewSettings
            {
                MonthViewLabelSetting = labelSettings,
                PreviousMonthBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                WeekEndBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                WeekDayBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                DayHeight = 0,
                CurrentMonthBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                TodayTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                WeekEndTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                InlineTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                DisabledTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                CurrentMonthTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                WeekDayTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                DateSelectionColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                SelectedDayTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                TodaySelectionTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                TodaySelectionBackgroundColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                PreviousMonthTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue))
            };

            YearViewSettings yearViewSettings = new YearViewSettings
            {
                MonthHeaderBackground = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                YearHeaderBackground = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                YearLayoutBackground = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)),
                YearHeaderTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.white)),
                MonthHeaderTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.white)),
                DateTextColor = new Color(ContextCompat.GetColor(context, Resource.Color.white)),
            };

            ShowEventsInline = false;
            HeaderHeight = 100;
            ViewMode = ViewMode.YearView;
            MonthViewSettings = monthViewSettings;
            YearViewSettings = yearViewSettings;

            SetPadding(15, 15, 15, 15);
        }
    }
}