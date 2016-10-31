//
// Project: Mark5.Mobile.Droid
// File: ContactsSearchFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.SearchViews;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ContactsSearchFragment : RetainableStateFragment
    {

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactsSearchFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search_contacts);

            CommonConfig.Logger.Info($"Created {nameof(ContactsSearchFragment)}");
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Add(Menu.None, Menu.None, Menu.First, Resource.String.search);
            menu.GetItem(0).SetIcon(Android.Resource.Drawable.IcMenuSearch);
            menu.GetItem(0).SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var i = new Intent(Activity, typeof(SearchResultsActivity));
            i.PutExtra(SearchResultsActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Contacts));
            i.PutExtra(SearchResultsActivity.CriteriaIntentKey, SerializationUtils.Serialize(GetCriteria()));
            StartActivity(i);

            return true;
        }

        SearchContactsCriteria GetCriteria()
        {
            var criteria = new SearchContactsCriteria();

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                var dsv = linearLayout.GetChildAt(i) as AbstractSearchView<SearchContactsCriteria>;
                if (dsv != null)
                {
                    dsv.ToCriteria(criteria);
                }
            }

            return criteria;
        }

        void SetCriteria(SearchContactsCriteria criteria)
        {
            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                var dsv = linearLayout.GetChildAt(i) as AbstractSearchView<SearchContactsCriteria>;
                if (dsv != null)
                {
                    dsv.FromCriteria(criteria);
                }
            }
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new ContactsSearchFragmentState { Criteria = GetCriteria() };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dsfs = restoredState as ContactsSearchFragmentState;
            if (dsfs != null)
            {
                SetCriteria(dsfs.Criteria);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ContactsSearchFragment)}";
        }

        class ContactsSearchFragmentState : IRetainableState
        {

            public SearchContactsCriteria Criteria { get; set; }
        }
    }
}

