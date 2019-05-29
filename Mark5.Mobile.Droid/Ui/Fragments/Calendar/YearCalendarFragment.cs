using System;
using Android.OS;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Com.Syncfusion.Calendar;
using Com.Syncfusion.Calendar.Enums;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Coordinators;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;
using Android.Widget;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class YearCalendarFragment : BaseFragment
    {
        const string InitialDateKey = "InitialDateKey";

        ICalendarCoordinator coordinator;
        YearCalendar yearCalendar;

        Java.Util.Calendar initialDate;

        public static (YearCalendarFragment fragment, string tag) NewInstance(Java.Util.Calendar initialDate)
        {
            var args = new Bundle();

            if (initialDate != null)
                args.PutString(InitialDateKey, Serializer.Serialize(initialDate.ConvertToDateTime()));

            var fragment = new YearCalendarFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(YearCalendarFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            coordinator = ((MainActivity)Activity).CalendarCoordinator;

            if (Arguments.ContainsKey(InitialDateKey))
                initialDate = Serializer.Deserialize<DateTime>(Arguments.GetString(InitialDateKey)).ConvertToCalendar();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;

            int paddingValue = Conversion.ConvertDpToPixels(15);
            var externalLayout = new FrameLayout(Context);
            externalLayout.SetBackgroundResource(Resource.Color.darkerblue);
            externalLayout.SetPadding(paddingValue, paddingValue, paddingValue, paddingValue);

            yearCalendar = new YearCalendar(Context);
            yearCalendar.MonthChanged += YearCalendar_MonthChanged;
            yearCalendar.MoveToDate = initialDate;
            externalLayout.AddView(yearCalendar);

            return externalLayout;
        }

        void YearCalendar_MonthChanged(object sender, MonthChangedEventArgs e)
        {
            coordinator.MonthTapped(e.NewValue);
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
        }
    }
}