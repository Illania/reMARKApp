//
// Project: Mark5.Mobile.Droid
// File: DateRangeSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class DateRangeSearchView : DocumentsSearchView
    {

        readonly AppCompatSpinner dateRangeType;
        readonly LinearLayoutCompat fromToLayout;
        readonly AppCompatTextView dateRangeFrom;
        readonly AppCompatTextView dateRangeTo;

        long fromTimestamp;
        long toTimestamp;

        public DateRangeSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceLarge);

            dateRangeType = new AppCompatSpinner(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Adapter = CustomArrayAdapter.CreateWithoutLeftPadding(context, Resource.Array.search_date_range_type, Android.Resource.Layout.SimpleSpinnerItem, Resource.Layout.support_simple_spinner_dropdown_item)
            };
            dateRangeType.SetSelection(0);
            dateRangeType.ItemSelected += (sender, e) => UpdateFromToFields(e.Position);
            AddView(dateRangeType);

            fromToLayout = new LinearLayoutCompat(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    TopMargin = DistanceNormal
                },
                Orientation = Horizontal
            };
            fromToLayout.Visibility = ViewStates.Gone;
            AddView(fromToLayout);

            dateRangeFrom = new AppCompatTextView(context, null, Resource.Attribute.spinnerStyle)
            {
                LayoutParameters = new LayoutParams(-1, ViewGroup.LayoutParams.WrapContent)
                {
                    RightMargin = DistanceNormal,
                    Weight = 50
                }
            };
            dateRangeFrom.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            dateRangeFrom.Clickable = true;
            dateRangeFrom.Click += async (sender, e) =>
            {
                fromTimestamp = await Dialogs.ShowDatePicker(context, fromTimestamp, maxTimestamp: toTimestamp);
                UpdateFromToFields();
            };
            fromToLayout.AddView(dateRangeFrom);

            dateRangeTo = new AppCompatTextView(context, null, Resource.Attribute.spinnerStyle)
            {
                LayoutParameters = new LayoutParams(-1, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = DistanceNormal,
                    Weight = 50
                }
            };
            dateRangeTo.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            dateRangeTo.Clickable = true;
            dateRangeTo.Click += async (sender, e) =>
            {
                toTimestamp = await Dialogs.ShowDatePicker(context, toTimestamp, fromTimestamp);
                UpdateFromToFields();
            };
            fromToLayout.AddView(dateRangeTo);

            UpdateFromToFields(dateRangeType.SelectedItemPosition);
        }

        void UpdateFromToFields(int mode = -1)
        {
            if (mode == 0)
            {
                fromToLayout.Visibility = ViewStates.Gone;
                fromTimestamp = -1;
                toTimestamp = -1;
            }
            if (mode == 1)
            {
                fromToLayout.Visibility = ViewStates.Gone;
                var now = DateTime.UtcNow.Date;
                fromTimestamp = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
                toTimestamp = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
            }
            if (mode == 2)
            {
                fromToLayout.Visibility = ViewStates.Gone;
                var yesterday = DateTime.UtcNow.Date.AddDays(-1);
                fromTimestamp = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
                toTimestamp = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
            }
            if (mode == 3)
            {
                fromToLayout.Visibility = ViewStates.Visible;
                var now = DateTime.UtcNow.Date;
                var weekAgo = now.AddDays(-7);
                fromTimestamp = new DateTime(weekAgo.Year, weekAgo.Month, weekAgo.Day, 0, 0, 0, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
                toTimestamp = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
            }

            dateRangeFrom.Text = fromTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToServerTime().ConvertDateTimeToTimestampMilliseconds().FormatServerTimestampAsDateString(Context);
            dateRangeTo.Text = toTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToServerTime().ConvertDateTimeToTimestampMilliseconds().FormatServerTimestampAsDateString(Context);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            if (criteria.DateRange == null || !criteria.DateRange.Enabled)
            {
                UpdateFromToFields(0);
            }
            else
            {
                fromTimestamp = criteria.DateRange.StartTimestamp;
                toTimestamp = criteria.DateRange.EndTimestamp;
                UpdateFromToFields(3);
            }
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.DateRange = new DateRange
            {
                Enabled = dateRangeType.SelectedItemPosition != 0,
                StartTimestamp = fromTimestamp,
                EndTimestamp = toTimestamp
            };
        }
    }
}
