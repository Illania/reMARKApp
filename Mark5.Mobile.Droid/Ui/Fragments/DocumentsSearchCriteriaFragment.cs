//
// Project: Mark5.Mobile.Droid
// File: SearchFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
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

            var dsv = new DocumentDateRangeSearchView(Context);
            dsv.TransitionName = "prova";

            linearLayout.AddView(new DocumentDirectionsSearchView(Context));
            linearLayout.AddView(new DocumentSubjectMessageSearchView(Context));
            linearLayout.AddView(new DocumentFromToSearchView(Context));
            linearLayout.AddView(dsv);
            PrepareEditableTextRow();
            PrepareDropdownTextRow();
            linearLayout.AddView(new DocumentAttachmentUnreadSearchView(Context));
            linearLayout.AddView(new DocumentHandledSearchView(Context));

            return rootView;
        }

        DocumentReferenceNumberSearchView drf;
        DocumentCommentsSearchView dcs;
        DocumentAttachmentSearchView das;

        public void PrepareEditableTextRow()
        {
            var ll = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            ll.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ll.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;

            var lp = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);

            dcs = new DocumentCommentsSearchView(Context, ll);
            das = new DocumentAttachmentSearchView(Context, ll);
            drf = new DocumentReferenceNumberSearchView(Context, ll);

            ll.AddView(drf, lp);
            ll.AddView(dcs, lp);
            ll.AddView(das, lp);
            ll.LayoutTransition = new Android.Animation.LayoutTransition();

            linearLayout.AddView(ll);
        }

        public void PrepareDropdownTextRow()
        {
            var ll = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            ll.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ll.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;

            var lp = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);

            ll.AddView(new DocumentCategoriesSearchView(Context, this), lp);
            ll.AddView(new DocumentLinesSearchView(Context, this), lp);
            ll.AddView(new DocumentPrioritySearchView(Context, this), lp);

            linearLayout.AddView(ll);
        }

        public void PushDropdownViewFragment(Fragment foldersListFragment, string tag)
        {
            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;

            fragmentManager.BeginTransaction()
                                       .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                                       .Add(Resource.Id.fragment_container, foldersListFragment, tag) //TODO this needs to be replace, but we need to do something about the criteria
                                       .AddToBackStack(tag)
                                       .Commit();
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