//
// Project: Mark5.Mobile.Droid
// File: AbstractDateRangeSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public abstract class AbstractDateRangeSearchView<T> : AbstractSearchView<T>
    {

        protected readonly AppCompatSpinner DateRangeType;
        protected readonly LinearLayoutCompat FromToLayout;
        protected readonly AppCompatTextView DateRangeFrom;
        protected readonly AppCompatTextView DateRangeTo;

        protected long FromTimestamp = -1;
        protected long ToTimestamp = -1;

        protected AbstractDateRangeSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceLarge);

            DateRangeType = new AppCompatSpinner(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Adapter = CustomArrayAdapter.CreateWithoutLeftPadding(context, Resource.Array.search_date_range_type, Android.Resource.Layout.SimpleSpinnerItem, Resource.Layout.support_simple_spinner_dropdown_item)
            };
            DateRangeType.SetSelection(0);
            DateRangeType.ItemSelected += (sender, e) =>
            {
                UpdateTimestamps();
                UpdateText();
            };
            AddView(DateRangeType);

            FromToLayout = new LinearLayoutCompat(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    TopMargin = DistanceNormal
                },
                Orientation = Horizontal
            };
            FromToLayout.Visibility = ViewStates.Gone;
            AddView(FromToLayout);

            DateRangeFrom = new AppCompatTextView(context, null, Resource.Attribute.spinnerStyle)
            {
                LayoutParameters = new LayoutParams(-1, ViewGroup.LayoutParams.WrapContent)
                {
                    RightMargin = DistanceNormal,
                    Weight = 50
                }
            };
            DateRangeFrom.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            DateRangeFrom.Clickable = true;
            DateRangeFrom.Click += async (sender, e) =>
            {
                FromTimestamp = await Dialogs.ShowDatePicker(context, FromTimestamp, maxTimestamp: ToTimestamp);
                UpdateText();
            };
            FromToLayout.AddView(DateRangeFrom);

            DateRangeTo = new AppCompatTextView(context, null, Resource.Attribute.spinnerStyle)
            {
                LayoutParameters = new LayoutParams(-1, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = DistanceNormal,
                    Weight = 50
                }
            };
            DateRangeTo.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            DateRangeTo.Clickable = true;
            DateRangeTo.Click += async (sender, e) =>
            {
                ToTimestamp = await Dialogs.ShowDatePicker(context, ToTimestamp, FromTimestamp);
                UpdateText();
            };
            FromToLayout.AddView(DateRangeTo);

            UpdateTimestamps();
            UpdateText();
        }

        protected void UpdateTimestamps()
        {
            var mode = DateRangeType.SelectedItemPosition;

            if (mode == 0)
            {
                FromToLayout.Visibility = ViewStates.Gone;
                FromTimestamp = -1;
                ToTimestamp = -1;
            }
            if (mode == 1)
            {
                FromToLayout.Visibility = ViewStates.Gone;
                var now = DateTime.UtcNow.Date;
                FromTimestamp = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
                ToTimestamp = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
            }
            if (mode == 2)
            {
                FromToLayout.Visibility = ViewStates.Gone;
                var yesterday = DateTime.UtcNow.Date.AddDays(-1);
                FromTimestamp = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
                ToTimestamp = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
            }
            if (mode == 3)
            {
                FromToLayout.Visibility = ViewStates.Visible;
                var now = DateTime.UtcNow.Date;
                var weekAgo = now.AddDays(-7);
                FromTimestamp = new DateTime(weekAgo.Year, weekAgo.Month, weekAgo.Day, 0, 0, 0, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
                ToTimestamp = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Utc).ConvertDateTimeToTimestampMilliseconds();
            }
        }

        protected void UpdateText()
        {
            DateRangeFrom.Text = FromTimestamp.FormatServerTimestampAsDateString(Context);
            DateRangeTo.Text = ToTimestamp.FormatServerTimestampAsDateString(Context);
        }
    }
}
