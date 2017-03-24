//
// Project: Mark5.Mobile.Droid
// File: SearchFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
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
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DocumentSearchCriteriaFragment : RetainableStateFragment
    {

        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentSearchCriteriaFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            var scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.SetBackgroundColor(Color.Transparent);

            linearLayout.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_horizontal);
            linearLayout.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;

            var paddingLinearLayout = ConversionUtils.ConvertDpToPixels(12);
            linearLayout.SetPadding(paddingLinearLayout, paddingLinearLayout, paddingLinearLayout, paddingLinearLayout);

            linearLayout.AddView(new AbstractButtonsView(Context));
            linearLayout.AddView(new AbstractMultiView(Context));
            PrepareDropdownRow();
            linearLayout.AddView(new AbstractEditableTextView(Context));
            linearLayout.AddView(new DateRangeView(Context));

            return rootView;
        }

        public void PrepareDropdownRow()
        {
            var ll = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            var lp = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);

            var a = new AbstractDropdownView(Context);
            var b = new AbstractDropdownView(Context);
            var c = new AbstractDropdownView(Context);

            ll.AddView(a, lp);
            ll.AddView(b, lp);
            ll.AddView(c, lp);

            linearLayout.AddView(ll);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.documents);

            CommonConfig.Logger.Info($"Created {nameof(DocumentSearchCriteriaFragment)}");
        }

        public override string GenerateTag()
        {
            return $"{nameof(DocumentSearchCriteriaFragment)}";
        }
    }
}