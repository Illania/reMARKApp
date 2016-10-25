//
// Project: Mark5.Mobile.Droid
// File: DateRangeSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
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
            dateRangeType.ItemSelected += (sender, e) =>
            {
                fromToLayout.Visibility = e.Position == 3 ? ViewStates.Visible : ViewStates.Gone;
            };
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
            dateRangeFrom.Text = "22/12/2022";
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
            dateRangeTo.Text = "22/12/2022";
            fromToLayout.AddView(dateRangeTo);
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
        }
    }
}
