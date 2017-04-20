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

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickDateRangeFragment : RetainableStateFragment
    {
        DocumentPickDateHeaderView dateHeaderView;

        CalendarView fromCalendar;
        CalendarView toCalendar;

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

            fromCalendar = new CalendarView(Context);
            fromCalendar.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            fromCalendar.Date = DateTime.UtcNow.Date.ConvertDateTimeToTimestampMilliseconds();
            fromCalendar.Visibility = ViewStates.Gone;
            fromCalendar.DateChange += FromDatePicker_DateChange;
            containerLinearLayout.AddView(fromCalendar);

            toCalendar = new CalendarView(Context);
            toCalendar.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            toCalendar.Visibility = ViewStates.Gone;
            toCalendar.DateChange += ToDatePicker_DateChange;
            containerLinearLayout.AddView(toCalendar);

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
            Initialize();
        }

        void Initialize()
        {
            var todayTimeStamp = DateTime.UtcNow.Date.ConvertDateTimeToTimestampMilliseconds();

            fromCalendar.MaxDate = todayTimeStamp;
            toCalendar.MaxDate = todayTimeStamp;

            fromCalendar.Date = FromTimestamp == -1 ? todayTimeStamp : FromTimestamp;
            toCalendar.Date = ToTimestamp == -1 ? todayTimeStamp : ToTimestamp;

            UpdateDatePickersLimits();

            UpdateText();

            if (StartWithToDate)
            {
                SelectTo();
            }
            else
            {
                SelectFrom();
            }
        }

        void DateView_FromClicked(object sender, EventArgs e)
        {
            if (fromCalendar.Visibility == ViewStates.Visible)
                return;

            SelectFrom();
        }

        void DateView_ToClicked(object sender, EventArgs e)
        {
            if (toCalendar.Visibility == ViewStates.Visible)
                return;

            SelectTo();
        }

        void SelectFrom()
        {
            fromCalendar.Visibility = ViewStates.Visible;
            toCalendar.Visibility = ViewStates.Gone;

            dateHeaderView.PickFrom();
        }

        void SelectTo()
        {
            fromCalendar.Visibility = ViewStates.Gone;
            toCalendar.Visibility = ViewStates.Visible;

            dateHeaderView.PickTo();
        }

        void FromDatePicker_DateChange(object sender, CalendarView.DateChangeEventArgs e)
        {
            FromTimestamp = new DateTime(e.Year, e.Month + 1, e.DayOfMonth, 0, 0, 1, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();

            UpdateText();
            UpdateDatePickersLimits();

            SelectTo();
        }

        void ToDatePicker_DateChange(object sender, CalendarView.DateChangeEventArgs e)
        {
            ToTimestamp = new DateTime(e.Year, e.Month + 1, e.DayOfMonth, 23, 59, 59, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();

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
                toCalendar.MinDate = FromTimestamp;
            }
        }

        void CloseFragment()
        {
            if (CloseRequest != null) CloseRequest(FromTimestamp, ToTimestamp);
            ((AppCompatActivity)Activity).OnBackPressed();
        }

        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new PickDateRangeFragmentState
            {
                FromTimestamp = FromTimestamp,
                ToTimestamp = ToTimestamp,
                StartWithToDate = toCalendar.Visibility == ViewStates.Visible,
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
