//
// Project: Mark5.Mobile.Droid
// File: ShortcodesSearchCriteriaFragment.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.SearchViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ShortcodesSearchCriteriaFragment : RetainableStateFragment, ISearchCriteriaFragment, View.IOnLayoutChangeListener
    {
        SearchShortcodesCriteria searchCriteria;

        LinearLayoutCompat containerLinearLayout;
        FloatingActionButton fab;

        List<AbstractSearchView<SearchShortcodesCriteria>> subviews = new List<AbstractSearchView<SearchShortcodesCriteria>>();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodesSearchCriteriaFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            var scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            containerLinearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            containerLinearLayout.SetBackgroundColor(Color.Transparent);
            containerLinearLayout.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_horizontal);
            containerLinearLayout.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;
            containerLinearLayout.Focusable = true;
            containerLinearLayout.FocusableInTouchMode = true;

            var paddingLinearLayout = ConversionUtils.ConvertDpToPixels(12);
            var bottomPadding = ConversionUtils.ConvertDpToPixels(56) + (Resources.GetDimension(Resource.Dimension.fab_margin) + 2) * 2;
            containerLinearLayout.SetPadding(paddingLinearLayout, paddingLinearLayout, paddingLinearLayout, (int)bottomPadding);

            fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.AddOnLayoutChangeListener(new FloatingActionButtonLayoutChangeListener());

            var fabIcon = Resources.GetDrawable(Resource.Drawable.action_search_server, null).GetConstantState().NewDrawable().Mutate();
            fabIcon.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)), PorterDuff.Mode.Multiply);

            fab.SetImageDrawable(fabIcon);
            fab.SetOnClickListener(new ActionOnClickListener(HandleSearchButtonClicked));
            fab.BackgroundTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));
            fab.RippleColor = new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)).ToArgb();
            fab.Visibility = ViewStates.Visible;

            var p = (CoordinatorLayout.LayoutParams)fab.LayoutParameters;
            p.Gravity = (int)(GravityFlags.Bottom | GravityFlags.CenterHorizontal);
            p.Behavior = new FloatingActionButtonBehavior();
            fab.LayoutParameters = p;

            var nameCriteria = new ShortcodeNameSearchView(Context);
            subviews.Add(nameCriteria);

            var emailCriteria = new ShortcodeAddressSearchView(Context);
            subviews.Add(emailCriteria);

            var descriptionCriteria = new ShortcodeDescriptionSearchView(Context);
            subviews.Add(descriptionCriteria);

            containerLinearLayout.AddView(nameCriteria);
            containerLinearLayout.AddView(emailCriteria);
            containerLinearLayout.AddView(descriptionCriteria);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.shortcodes);

            CommonConfig.Logger.Info($"Created {nameof(ShortcodesSearchCriteriaFragment)}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            fab.Visibility = ViewStates.Visible;

            try
            {
                searchCriteria = searchCriteria ?? await Managers.SearchManager.GetLastSearchShortcodesCrtieriaAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to restore last search criteria", ex);

                searchCriteria = new SearchShortcodesCriteria();
            }

            RefreshViews();
        }

        public override void OnPause()
        {
            fab.Visibility = ViewStates.Gone;
            base.OnPause();
        }

        public override async void OnStop()
        {
            base.OnStop();

            try
            {
                await Managers.SearchManager.SaveLastSearchShortcodesCrtieriaAsync(searchCriteria);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to clear last search criteria", ex);
            }
        }

        void RefreshViews()
        {
            subviews.ForEach(c =>
            {
                c.Criteria = searchCriteria;
                c.Refresh();
            });
        }

        async void Reset()
        {
            searchCriteria = new SearchShortcodesCriteria();
            containerLinearLayout.RequestFocus();
            ((InputMethodManager)Context.GetSystemService(Context.InputMethodService)).HideSoftInputFromWindow(containerLinearLayout.WindowToken, HideSoftInputFlags.None);
            RefreshViews();

            try
            {
                await Managers.SearchManager.SaveLastSearchShortcodesCrtieriaAsync(searchCriteria);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to clear last search criteria", ex);
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();
            var item = menu.Add(Menu.None, 10, 10, Resource.String.reset);
            item.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                Reset();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public void ReplaceFragment(Fragment f, string tag)
        {
            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;

            fragmentManager.BeginTransaction()
                                       .SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right)
                                       .Replace(Resource.Id.fragment_container, f, tag)
                                       .AddToBackStack(tag)
                                       .Commit();
        }

        void HandleSearchButtonClicked()
        {
            GetCriteria();

            var i = new Intent(Activity, typeof(SearchResultsActivity));
            i.PutExtra(SearchResultsActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Shortcodes));
            i.PutExtra(SearchResultsActivity.CriteriaIntentKey, SerializationUtils.Serialize(GetCriteria()));
            StartActivity(i);
        }

        SearchShortcodesCriteria GetCriteria()
        {
            subviews.ForEach(v => v.UpdateCriteria());

            searchCriteria.MaxToFetch = PlatformConfig.Preferences.MaxShortcodesToSearch;

            CommonConfig.Logger.Info($"Starting search... [criteria={SerializationUtils.Serialize(searchCriteria)}]");

            return searchCriteria;
        }

        public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
        {
            var parent = containerLinearLayout?.Parent?.Parent?.Parent?.Parent?.Parent as CoordinatorLayout;

            if (parent == null)
                return;

            var distance = parent.Bottom - v.Bottom;
            var bottomMargin = v.Context.Resources.GetDimension(Resource.Dimension.fab_margin);

            v.Visibility = distance > bottomMargin * 2 ? ViewStates.Invisible : ViewStates.Visible;
        }

        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new ShortcodeSearchCriteriaFragmentState
            {
                Criteria = GetCriteria(),
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var df = restoredState as ShortcodeSearchCriteriaFragmentState;
            if (df != null)
            {
                searchCriteria = df.Criteria;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ShortcodesSearchCriteriaFragment)}";
        }

        class ShortcodeSearchCriteriaFragmentState : IRetainableState
        {
            public SearchShortcodesCriteria Criteria { get; set; }
        }

        #endregion
    }
}
