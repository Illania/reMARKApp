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
        long fromTimestamp = -1;
        long toTimestamp = -1;

        DocumentDateRangeSearchView dateView;

        CalendarView fromDatePicker;
        CalendarView toDatePicker;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            var scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));

            var containerLinearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            containerLinearLayout.SetBackgroundColor(Color.Transparent);
            containerLinearLayout.LayoutTransition = new LayoutTransition();

            dateView = new DocumentDateRangeSearchView(Context, null, true);
            containerLinearLayout.AddView(dateView);

            fromDatePicker = new CalendarView(Context);
            fromDatePicker.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            fromDatePicker.Date = DateTime.UtcNow.Date.ConvertDateTimeToTimestampMilliseconds();
            fromDatePicker.DateChange += FromDatePicker_DateChange;
            containerLinearLayout.AddView(fromDatePicker);

            toDatePicker = new CalendarView(Context);
            toDatePicker.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            toDatePicker.Visibility = ViewStates.Gone;
            toDatePicker.DateChange += ToDatePicker_DateChange;
            containerLinearLayout.AddView(toDatePicker);

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

            fromDatePicker.MaxDate = todayTimeStamp;
            toDatePicker.MaxDate = todayTimeStamp;

            fromDatePicker.Date = fromTimestamp == -1 ? todayTimeStamp : fromTimestamp;
            toDatePicker.Date = toTimestamp == -1 ? todayTimeStamp : toTimestamp;

            UpdateText();
        }

        void FromDatePicker_DateChange(object sender, CalendarView.DateChangeEventArgs e)
        {
            CommonConfig.Logger.Error($"{e.DayOfMonth} - {e.Month} - {e.Year}");

            fromTimestamp = fromDatePicker.Date;

            UpdateText();
            UpdateDatePickersLimits();

            fromDatePicker.Visibility = ViewStates.Gone;
            toDatePicker.Visibility = ViewStates.Visible;
        }

        void ToDatePicker_DateChange(object sender, CalendarView.DateChangeEventArgs e)
        {
            toTimestamp = toDatePicker.Date;
            UpdateText();

            //TODO should close? need to ask linnea
        }

        void UpdateText()
        {
            dateView.SetToText(toTimestamp);
            dateView.SetFromText(fromTimestamp);
        }

        void UpdateDatePickersLimits()
        {
            if (fromTimestamp != -1)
            {
                toDatePicker.MinDate = fromTimestamp;
            }
        }

        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new PickDateRangeFragmentState
            {
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as PickDateRangeFragmentState;
            if (clfs != null)
            {
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(PickDateRangeFragment)} ]";
        }

        class PickDateRangeFragmentState : IRetainableState
        {

        }

        #endregion
    }
}
