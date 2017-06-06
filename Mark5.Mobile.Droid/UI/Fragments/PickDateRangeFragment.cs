//
// Project: Mark5.Mobile.Droid
// File: PickDateRangeFragment.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.SearchViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickDateRangeFragment : RetainableStateFragment
    {
        DocumentPickDateHeaderView dateHeaderView;

        CalendarView fromCalendarView;
        CalendarView toCalendarView;

        public Action<long, long> CloseRequest { get; set; }
        public bool StartWithToDate { get; set; }
        public long FromTimestamp { get; set; }
        public long ToTimestamp { get; set; }

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

            fromCalendarView = new CalendarView(Context);
            fromCalendarView.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            fromCalendarView.Date = DateTime.UtcNow.Date.ConvertDateTimeToTimestampMilliseconds();
            fromCalendarView.Visibility = ViewStates.Gone;
            fromCalendarView.DateChange += FromDatePicker_DateChange;
            containerLinearLayout.AddView(fromCalendarView);

            toCalendarView = new CalendarView(Context);
            toCalendarView.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            toCalendarView.Visibility = ViewStates.Gone;
            toCalendarView.DateChange += ToDatePicker_DateChange;
            containerLinearLayout.AddView(toCalendarView);

            HasOptionsMenu = false;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = GetString(Resource.String.date);

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
            fromLimitCalendar.Set(Java.Util.CalendarField.HourOfDay, 0);
            fromLimitCalendar.Set(Java.Util.CalendarField.Minute, 0);
            fromLimitCalendar.Set(Java.Util.CalendarField.Second, 0);

            var toLimitCalendar = Java.Util.Calendar.Instance;
            toLimitCalendar.Set(Java.Util.CalendarField.Year, now.Year);
            toLimitCalendar.Set(Java.Util.CalendarField.Month, now.Month - 1);
            toLimitCalendar.Set(Java.Util.CalendarField.DayOfMonth, now.Day);
            toLimitCalendar.Set(Java.Util.CalendarField.HourOfDay, 23);
            toLimitCalendar.Set(Java.Util.CalendarField.Minute, 59);
            toLimitCalendar.Set(Java.Util.CalendarField.Second, 59);

            fromCalendarView.MaxDate = toLimitCalendar.TimeInMillis;
            toCalendarView.MaxDate = toLimitCalendar.TimeInMillis;

            var fromDate = FromTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
            var fromDateCalendar = Java.Util.Calendar.Instance;
            fromDateCalendar.Set(Java.Util.CalendarField.Year, fromDate.Year);
            fromDateCalendar.Set(Java.Util.CalendarField.Month, fromDate.Month - 1);
            fromDateCalendar.Set(Java.Util.CalendarField.DayOfMonth, fromDate.Day);
            fromDateCalendar.Set(Java.Util.CalendarField.HourOfDay, 0);
            fromDateCalendar.Set(Java.Util.CalendarField.Minute, 0);
            fromDateCalendar.Set(Java.Util.CalendarField.Second, 1);

            var toDate = ToTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
            var toDateCalendar = Java.Util.Calendar.Instance;
            toDateCalendar.Set(Java.Util.CalendarField.Year, toDate.Year);
            toDateCalendar.Set(Java.Util.CalendarField.Month, toDate.Month - 1);
            toDateCalendar.Set(Java.Util.CalendarField.DayOfMonth, toDate.Day);
            toDateCalendar.Set(Java.Util.CalendarField.HourOfDay, 23);
            toDateCalendar.Set(Java.Util.CalendarField.Minute, 59);
            toDateCalendar.Set(Java.Util.CalendarField.Second, 59);

            fromCalendarView.Date = FromTimestamp == -1 ? fromLimitCalendar.TimeInMillis : fromDateCalendar.TimeInMillis;
            toCalendarView.Date = ToTimestamp == -1 ? toLimitCalendar.TimeInMillis : toDateCalendar.TimeInMillis;

            UpdateDatePickersLimits();
            UpdateText();

            if (StartWithToDate)
                SelectTo();
            else
                SelectFrom();
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

        void FromDatePicker_DateChange(object sender, CalendarView.DateChangeEventArgs e)
        {
            FromTimestamp = new DateTime(e.Year, e.Month + 1, e.DayOfMonth, 0, 0, 0, DateTimeKind.Unspecified).ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();

            UpdateText();
            UpdateDatePickersLimits();

            SelectTo();
        }

        void ToDatePicker_DateChange(object sender, CalendarView.DateChangeEventArgs e)
        {
            ToTimestamp = new DateTime(e.Year, e.Month + 1, e.DayOfMonth, 23, 59, 59, DateTimeKind.Unspecified).ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();

            UpdateText();
            CloseFragment();
        }

        void UpdateText()
        {
            dateHeaderView.SetToText(ToTimestamp);
            dateHeaderView.SetFromText(FromTimestamp);
        }

        void UpdateDatePickersLimits()
        {
            if (FromTimestamp != -1)
            {
                var fromDate = FromTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime();
                var fromDateCalendar = Java.Util.Calendar.Instance;
                fromDateCalendar.Set(Java.Util.CalendarField.Year, fromDate.Year);
                fromDateCalendar.Set(Java.Util.CalendarField.Month, fromDate.Month - 1);
                fromDateCalendar.Set(Java.Util.CalendarField.DayOfMonth, fromDate.Day);
                fromDateCalendar.Set(Java.Util.CalendarField.HourOfDay, 0);
                fromDateCalendar.Set(Java.Util.CalendarField.Minute, 0);
                fromDateCalendar.Set(Java.Util.CalendarField.Second, 1);

                toCalendarView.MinDate = fromDateCalendar.TimeInMillis;

                if (toCalendarView.Date < toCalendarView.MinDate)
                    toCalendarView.Date = toCalendarView.MinDate;
            }
        }

        void CloseFragment()
        {
            if (CloseRequest != null)
                CloseRequest(FromTimestamp, ToTimestamp);
            ((AppCompatActivity) Activity).OnBackPressed();
        }

        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new PickDateRangeFragmentState
            {
                FromTimestamp = FromTimestamp,
                ToTimestamp = ToTimestamp,
                StartWithToDate = toCalendarView.Visibility == ViewStates.Visible,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var fs = restoredState as PickDateRangeFragmentState;
            if (fs != null)
            {
                FromTimestamp = fs.FromTimestamp;
                ToTimestamp = fs.ToTimestamp;
                StartWithToDate = fs.StartWithToDate;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(PickDateRangeFragment)}";
        }

        class PickDateRangeFragmentState : IRetainableState
        {
            public long FromTimestamp { get; set; }
            public long ToTimestamp { get; set; }
            public bool StartWithToDate { get; set; }
        }

        #endregion
    }
}