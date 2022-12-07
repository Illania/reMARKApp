using System;
using System.Threading.Tasks;
using Android.Animation;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.SearchViews;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickDateRangeFragment : BaseFragment
    {
        public Task<(long, long)> Task => tcs.Task;

        readonly TaskCompletionSource<(long, long)> tcs = new TaskCompletionSource<(long, long)>();

        const string FromTimestampBundleKey = "FromTimestamp_f9118547-848d-48fd-a9a8-b2d721494ef8";
        const string ToTimestampBundleKey = "ToTimestamp_680c200a-8617-412f-aec6-8c0c059db839";
        const string StartWithToDateBundleKey = "StartWithToDate_3ce7c7d1-0d5d-4821-8b17-cbc0a04889cf";

        DocumentPickDateHeaderView dateHeaderView;

        DatePicker fromCalendarView;
        DatePicker toCalendarView;

        long fromTimestamp;
        long toTimestamp;
        bool startWithToDate;

        public static (PickDateRangeFragment fragment, string tag) NewInstance(long? fromTimestamp, long? toTimestamp, bool? startWithToDate)
        {
            var args = new Bundle();

            if (fromTimestamp != null)
                args.PutLong(FromTimestampBundleKey, fromTimestamp.Value);

            if (toTimestamp != null)
                args.PutLong(ToTimestampBundleKey, toTimestamp.Value);

            if (startWithToDate != null)
                args.PutBoolean(StartWithToDateBundleKey, startWithToDate.Value);

            var fragment = new PickDateRangeFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(PickDateRangeFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null)
            {
                if (savedInstanceState.ContainsKey(FromTimestampBundleKey))
                    fromTimestamp = savedInstanceState.GetLong(FromTimestampBundleKey);

                if (savedInstanceState.ContainsKey(ToTimestampBundleKey))
                    toTimestamp = savedInstanceState.GetLong(ToTimestampBundleKey);

                if (savedInstanceState.ContainsKey(StartWithToDateBundleKey))
                    startWithToDate = savedInstanceState.GetBoolean(StartWithToDateBundleKey);
            }
            else
            {
                if (Arguments.ContainsKey(FromTimestampBundleKey))
                    fromTimestamp = Arguments.GetLong(FromTimestampBundleKey);

                if (Arguments.ContainsKey(ToTimestampBundleKey))
                    toTimestamp = Arguments.GetLong(ToTimestampBundleKey);

                if (Arguments.ContainsKey(StartWithToDateBundleKey))
                    startWithToDate = Arguments.GetBoolean(StartWithToDateBundleKey);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            var scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));

            var containerLinearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            containerLinearLayout.SetBackgroundColor(Color.Transparent);
            containerLinearLayout.LayoutTransition = new LayoutTransition();

            dateHeaderView = new DocumentPickDateHeaderView(Context);
            dateHeaderView.FromClicked += DateView_FromClicked;
            dateHeaderView.ToClicked += DateView_ToClicked;
            containerLinearLayout.AddView(dateHeaderView);

            fromCalendarView = new DatePicker(Context);
            fromCalendarView.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            fromCalendarView.DateTime = DateTime.UtcNow.Date;
            fromCalendarView.Visibility = ViewStates.Gone;

            containerLinearLayout.AddView(fromCalendarView);

            toCalendarView = new DatePicker(Context);
            toCalendarView.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            toCalendarView.DateTime = DateTime.UtcNow.Date;
            toCalendarView.Visibility = ViewStates.Gone;
            containerLinearLayout.AddView(toCalendarView);

            HasOptionsMenu = false;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.date);

            CommonConfig.Logger.Info($"Created {nameof(PickDateRangeFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            var now = DateTime.Now;

            var fromLimitCalendar = Java.Util.Calendar.Instance;
            fromLimitCalendar.Set(Java.Util.CalendarField.Year, now.Year);
            fromLimitCalendar.Set(Java.Util.CalendarField.Month, now.Month - 1);
            fromLimitCalendar.Set(Java.Util.CalendarField.DayOfMonth, now.Day);

            var toLimitCalendar = Java.Util.Calendar.Instance;
            toLimitCalendar.Set(Java.Util.CalendarField.Year, now.Year);
            toLimitCalendar.Set(Java.Util.CalendarField.Month, now.Month - 1);
            toLimitCalendar.Set(Java.Util.CalendarField.DayOfMonth, now.Day);

            fromCalendarView.MaxDate = toLimitCalendar.TimeInMillis;
            toCalendarView.MaxDate = toLimitCalendar.TimeInMillis;

            var fromDate = fromTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
            var fromDateCalendar = Java.Util.Calendar.Instance;
            fromDateCalendar.Set(Java.Util.CalendarField.Year, fromDate.Year);
            fromDateCalendar.Set(Java.Util.CalendarField.Month, fromDate.Month - 1);
            fromDateCalendar.Set(Java.Util.CalendarField.DayOfMonth, fromDate.Day);

            var toDate = toTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
            var toDateCalendar = Java.Util.Calendar.Instance;
            toDateCalendar.Set(Java.Util.CalendarField.Year, toDate.Year);
            toDateCalendar.Set(Java.Util.CalendarField.Month, toDate.Month - 1);
            toDateCalendar.Set(Java.Util.CalendarField.DayOfMonth, toDate.Day);

            fromCalendarView.DateTime =
                fromTimestamp == -1
                ? fromLimitCalendar.TimeInMillis.ConvertTimestampMillisecondsToDateTime()
                : fromDateCalendar.TimeInMillis.ConvertTimestampMillisecondsToDateTime();

            toCalendarView.DateTime =
                toTimestamp == -1
                ? toLimitCalendar.TimeInMillis.ConvertTimestampMillisecondsToDateTime()
                : toDateCalendar.TimeInMillis.ConvertTimestampMillisecondsToDateTime();
            
            UpdateDatePickersLimits();
            UpdateText();

            fromCalendarView.DateChanged += FromDatePicker_DateChange;
            toCalendarView.DateChanged += ToDatePicker_DateChange;

            if (startWithToDate)
                SelectTo();
            else
                SelectFrom();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutLong(FromTimestampBundleKey, fromTimestamp);
            outState.PutLong(ToTimestampBundleKey, toTimestamp);
            outState.PutBoolean(StartWithToDateBundleKey, toCalendarView.Visibility == ViewStates.Visible);
        }

        void DateView_FromClicked(object sender, EventArgs e)
        {
            if (fromCalendarView.Visibility == ViewStates.Visible)
                return;

            SelectFrom();
        }

        void DateView_ToClicked(object sender, EventArgs e)
        {
            if (toCalendarView.Visibility == ViewStates.Visible)
                return;

            SelectTo();
        }

        void SelectFrom()
        {
            fromCalendarView.Visibility = ViewStates.Visible;
            toCalendarView.Visibility = ViewStates.Gone;

            dateHeaderView.PickFrom();
        }

        void SelectTo()
        {
            fromCalendarView.Visibility = ViewStates.Gone;
            toCalendarView.Visibility = ViewStates.Visible;

            dateHeaderView.PickTo();
        }

        void FromDatePicker_DateChange(object sender, DatePicker.DateChangedEventArgs e)
        {
            fromTimestamp = new DateTime(e.Year, e.MonthOfYear + 1, e.DayOfMonth, 0, 0, 0, DateTimeKind.Unspecified)
                .ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();

            UpdateText();
            UpdateDatePickersLimits();

            SelectTo();
        }

        void ToDatePicker_DateChange(object sender, DatePicker.DateChangedEventArgs e)
        {
            toTimestamp = new DateTime(e.Year, e.MonthOfYear + 1, e.DayOfMonth, 23, 59, 59, DateTimeKind.Unspecified)
                .ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();

            UpdateText();
            CloseFragment();
        }

        void UpdateText()
        {
            dateHeaderView.SetToText(toTimestamp);
            dateHeaderView.SetFromText(fromTimestamp);
        }

        void UpdateDatePickersLimits()
        {
            if (fromTimestamp != -1)
            {
                var fromDate = fromTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
                var fromDateCalendar = Java.Util.Calendar.Instance;
                fromDateCalendar.Set(Java.Util.CalendarField.Year, fromDate.Year);
                fromDateCalendar.Set(Java.Util.CalendarField.Month, fromDate.Month - 1);
                fromDateCalendar.Set(Java.Util.CalendarField.DayOfMonth, fromDate.Day);
                fromDateCalendar.Set(Java.Util.CalendarField.HourOfDay, 0);
                fromDateCalendar.Set(Java.Util.CalendarField.Minute, 0);
                fromDateCalendar.Set(Java.Util.CalendarField.Second, 1);

                toCalendarView.MinDate = fromDateCalendar.TimeInMillis;

                if (toCalendarView.DateTime.ConvertDateTimeToTimestampMilliseconds() < toCalendarView.MinDate)
                    toCalendarView.DateTime = toCalendarView.MinDate.ConvertTimestampMillisecondsToDateTime();
            }
        }

        void CloseFragment()
        {
            tcs.SetResult((fromTimestamp, toTimestamp));
            ((AppCompatActivity)Activity).OnBackPressed();
        }
    }
}