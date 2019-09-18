using System;
using Android.OS;
using Android.Views;
using Android.Widget;
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
            externalLayout.SetBackgroundResource(Resource.Color.white);
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
        readonly Color darkerBlueColor;
        readonly Color whiteColor;
        readonly Color lightBlueColor;

        public YearCalendar(Context context) : base(context)
        {
            darkerBlueColor = new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue));
            whiteColor = new Color(ContextCompat.GetColor(context, Resource.Color.white));
            lightBlueColor = new Color(ContextCompat.GetColor(context, Resource.Color.lightblue));

            MonthViewLabelSetting labelSettings = new MonthViewLabelSetting
            {
                DateLabelSize = 10,
            };

            MonthViewSettings monthViewSettings = new MonthViewSettings
            {
                MonthViewLabelSetting = labelSettings,
                PreviousMonthBackgroundColor = whiteColor,
                WeekEndBackgroundColor = whiteColor,
                WeekDayBackgroundColor = whiteColor,
                DayHeight = 0,
                CurrentMonthBackgroundColor = whiteColor,
                TodayTextColor = darkerBlueColor,
                WeekEndTextColor = whiteColor,
                InlineTextColor = whiteColor,
                DisabledTextColor = whiteColor,
                CurrentMonthTextColor = darkerBlueColor,
                WeekDayTextColor = whiteColor,
                DateSelectionColor = whiteColor,
                SelectedDayTextColor = whiteColor,
                TodaySelectionTextColor = whiteColor,
                TodaySelectionBackgroundColor = whiteColor,
                PreviousMonthTextColor = whiteColor,
                BorderColor = darkerBlueColor
            };

            YearViewSettings yearViewSettings = new YearViewSettings
            {
                MonthHeaderBackground = whiteColor,
                YearHeaderBackground = whiteColor,
                YearLayoutBackground = whiteColor,
                YearHeaderTextColor = darkerBlueColor,
                MonthHeaderTextColor = darkerBlueColor,
                DateTextColor = darkerBlueColor,
            };

            ShowEventsInline = false;
            HeaderHeight = 100;
            ViewMode = ViewMode.YearView;
            YearViewMode = YearViewMode.Date;
            MonthViewSettings = monthViewSettings;
            YearViewSettings = yearViewSettings;
        }
    }
}