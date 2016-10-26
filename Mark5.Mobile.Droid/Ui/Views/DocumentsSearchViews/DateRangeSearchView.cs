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
using Android.Text.Format;
using Android.Views;
using Mark5.Mobile.Common.Model;
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

        readonly char[] dfo;

        DateTime from;
        DateTime to;

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

            dfo = DateFormat.GetDateFormatOrder(Context);

            dateRangeFrom = new AppCompatTextView(context, null, Resource.Attribute.spinnerStyle)
            {
                LayoutParameters = new LayoutParams(-1, ViewGroup.LayoutParams.WrapContent)
                {
                    RightMargin = DistanceNormal,
                    Weight = 50
                }
            };
            dateRangeFrom.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            dateRangeFrom.Text = from.ToString($"{dfo[0]}{dfo[0]}/{dfo[1]}{dfo[1]}/{dfo[2]}{dfo[2]}{dfo[2]}{dfo[2]}");
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
            dateRangeTo.Text = to.ToString($"{dfo[0]}{dfo[0]}/{dfo[1]}{dfo[1]}/{dfo[2]}{dfo[2]}{dfo[2]}{dfo[2]}");
            fromToLayout.AddView(dateRangeTo);

            UpdateFromToFields(dateRangeType.SelectedItemPosition);
        }

        void UpdateFromToFields(int mode)
        {
            fromToLayout.Visibility = mode == 3 ? ViewStates.Visible : ViewStates.Gone;

            if (mode == 0)
            {
                from = default(DateTime);
                to = default(DateTime);
            }
            if (mode == 1)
            {
                var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).Date;
                from = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Unspecified);
                to = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Unspecified);
            }
            if (mode == 2)
            {
                var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).Date;
                var yesterday = now.AddDays(-1);
                from = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0, DateTimeKind.Unspecified);
                to = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59, DateTimeKind.Unspecified);
            }
            if (mode == 3)
            {
                var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified).Date;
                var weekAgo = now.AddDays(-7);
                from = new DateTime(weekAgo.Year, weekAgo.Month, weekAgo.Day, 0, 0, 0, DateTimeKind.Unspecified);
                to = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Unspecified);
            }
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            if (criteria.DateRange == null || !criteria.DateRange.Enabled)
            {
                UpdateFromToFields(0);
            }
            else
            {
                from = criteria.DateRange.Start;
                to = criteria.DateRange.End;
                UpdateFromToFields(3);
            }
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.DateRange = new DateRange
            {
                Enabled = dateRangeType.SelectedItemPosition != 0,
                Start = from,
                End = to
            };
        }
    }
}
