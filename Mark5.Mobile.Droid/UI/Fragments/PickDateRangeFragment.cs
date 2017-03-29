//
// Project: Mark5.Mobile.Droid
// File: PickDateRangeFragment.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.SearchViews;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickDateRangeFragment : RetainableStateFragment
    {
        LinearLayoutCompat containerLinearLayout;
        DocumentDateRangeSearchView dateView;
        DatePicker datePicker;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            var scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));

            containerLinearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            containerLinearLayout.SetBackgroundColor(Color.Transparent);

            dateView = new DocumentDateRangeSearchView(Context, null, true);
            containerLinearLayout.AddView(dateView);

            datePicker = LayoutInflater.From(Context).Inflate(Resource.Layout.search_date_picker_layout, null).FindViewById<DatePicker>(Resource.Id.search_date_picker);
            containerLinearLayout.AddView(datePicker);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.date);

            CommonConfig.Logger.Info($"Created {nameof(PickDateRangeFragment)}");
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
