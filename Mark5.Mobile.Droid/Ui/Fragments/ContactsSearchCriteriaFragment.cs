using System;
using System.Collections.Generic;
using Android.Animation;
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
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.SearchViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ContactsSearchCriteriaFragment : BaseFragment, ISearchCriteriaFragment
    {
        const string SearchCriteriaKey = "SearchCriteria_8e4bff71-ffc2-42d6-9faf-c1d29717e7f2";

        SearchContactsCriteria searchCriteria;

        LinearLayoutCompat containerLinearLayout;
        FloatingActionButton fab;

        List<AbstractSearchView<SearchContactsCriteria>> subviews = new List<AbstractSearchView<SearchContactsCriteria>>();

        public static (ContactsSearchCriteriaFragment fragment, string tag) NewInstance()
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenSearchEvent());

            var fragment = new ContactsSearchCriteriaFragment();
            var tag = $"{nameof(ContactsSearchCriteriaFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null && savedInstanceState.ContainsKey(SearchCriteriaKey))
                searchCriteria = Serializer.Deserialize<SearchContactsCriteria>(savedInstanceState.GetString(SearchCriteriaKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactsSearchCriteriaFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            var scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            containerLinearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            containerLinearLayout.SetBackgroundColor(Color.Transparent);
            containerLinearLayout.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_horizontal);
            containerLinearLayout.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;
            containerLinearLayout.Focusable = true;
            containerLinearLayout.FocusableInTouchMode = true;

            var paddingLinearLayout = Conversion.ConvertDpToPixels(12);
            var bottomPadding = Conversion.ConvertDpToPixels(56) + (Resources.GetDimension(Resource.Dimension.fab_margin) + 2) * 2;
            containerLinearLayout.SetPadding(paddingLinearLayout, paddingLinearLayout, paddingLinearLayout, (int)bottomPadding);

            fab = ((BaseAppCompatActivity)Activity).Fab;
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

            var typeCriteria = new ContactTypeSearchView(Context);
            subviews.Add(typeCriteria);

            var nameCriteria = new ContactNameSearchView(Context);
            subviews.Add(nameCriteria);

            var emailCriteria = new ContactEmailSearchView(Context);
            subviews.Add(emailCriteria);

            containerLinearLayout.AddView(typeCriteria);
            containerLinearLayout.AddView(nameCriteria);
            containerLinearLayout.AddView(emailCriteria);
            PrepareEditableTextRow();
            PrepareDropdownTextRow();

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.contacts);

            CommonConfig.Logger.Info($"Created {nameof(ContactsSearchCriteriaFragment)}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            fab.Visibility = ViewStates.Visible;

            try
            {
                searchCriteria = searchCriteria ?? await Managers.SearchManager.GetLastSearchContactsCriteriaAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to restore last search criteria", ex);

                searchCriteria = new SearchContactsCriteria();
            }

            RefreshViews();
        }

        public override void OnPause()
        {
            fab.Visibility = ViewStates.Gone;
            UpdateCriteria();

            base.OnPause();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutString(SearchCriteriaKey, Serializer.Serialize(UpdateCriteria()));
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

        public override async void OnStop()
        {
            base.OnStop();

            try
            {
                await Managers.SearchManager.SaveLastSearchContactsCriteriaAsync(searchCriteria);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to clear last search criteria", ex);
            }
        }

        public void PrepareEditableTextRow()
        {
            var ll = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            ll.DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ll.ShowDividers = LinearLayoutCompat.ShowDividerMiddle;

            var lp = new LinearLayoutCompat.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);

            var shortIdCriteria = new ContactShortIdSearchView(Context, ll);
            subviews.Add(shortIdCriteria);

            var postAddressCriteria = new ContactPostAddressSearchView(Context, ll);
            subviews.Add(postAddressCriteria);

            var descriptionCriteria = new ContactDescriptionSearchView(Context, ll);
            subviews.Add(descriptionCriteria);

            ll.AddView(descriptionCriteria, lp);
            ll.AddView(shortIdCriteria, lp);
            ll.AddView(postAddressCriteria, lp);
            ll.LayoutTransition = new LayoutTransition();

            containerLinearLayout.AddView(ll);
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

            var countryCriteria = new ContactCountrySearchView(Context, this);
            subviews.Add(countryCriteria);

            var categoriesCriteria = new ContactCategoriesSearchView(Context, this);
            subviews.Add(categoriesCriteria);

            ll.AddView(countryCriteria, lp);
            ll.AddView(categoriesCriteria, lp);

            containerLinearLayout.AddView(ll);
        }

        void RefreshViews()
        {
            subviews.ForEach(c =>
            {
                c.Criteria = searchCriteria;
                c.Refresh();
            });
        }

        void HandleSearchButtonClicked()
        {
            UpdateCriteria();

            StartActivity(SearchResultsActivity.CreateIntent(Context, ModuleType.Contacts, UpdateCriteria()));
        }

        SearchContactsCriteria UpdateCriteria()
        {
            subviews.ForEach(v => v.UpdateCriteria());

            searchCriteria.MaxToFetch = PlatformConfig.Preferences.MaxContactsToSearch;

            CommonConfig.Logger.Info($"Starting search... [criteria={Serializer.Serialize(searchCriteria)}]");

            return searchCriteria;
        }

        async void Reset()
        {
            searchCriteria = new SearchContactsCriteria();
            containerLinearLayout.RequestFocus();
            ((InputMethodManager)Context.GetSystemService(Context.InputMethodService)).HideSoftInputFromWindow(containerLinearLayout.WindowToken, HideSoftInputFlags.None);
            RefreshViews();

            try
            {
                await Managers.SearchManager.SaveLastSearchContactsCriteriaAsync(searchCriteria);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to clear last search criteria", ex);
            }
        }

        public void ReplaceFragment(Fragment f, string tag)
        {
            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;

            fragmentManager.BeginTransaction().SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right).Replace(Resource.Id.fragment_container, f, tag).AddToBackStack(tag).Commit();
        }
    }
}