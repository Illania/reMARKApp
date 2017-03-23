//
// Project: Mark5.Mobile.Droid
// File: ContactsSearchCriteriaFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.SearchViews;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ContactsSearchCriteriaFragment : RetainableStateFragment
    {

        LinearLayoutCompat linearLayout;
        AppCompatButton searchButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactsSearchCriteriaFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_button, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            searchButton = rootView.FindViewById<AppCompatButton>(Resource.Id.button);

            linearLayout.AddView(new ContactNameSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContactFirstNameSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContactLastNameSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContactShortIdSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContactDescriptionSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContactTypeSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContactComAddressSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContactPostAddressSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContactCountrySearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContactCommentSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContactCategoriesSearchView(Context, this));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new MaxContactsSearchView(Context));

            searchButton.Text = GetString(Resource.String.search);
            searchButton.Click += (sender, e) =>
            {
                var i = new Intent(Activity, typeof(SearchResultsActivity));
                i.PutExtra(SearchResultsActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Contacts));
                i.PutExtra(SearchResultsActivity.CriteriaIntentKey, SerializationUtils.Serialize(GetCriteria()));
                StartActivity(i);
            };

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            //((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search_contacts);
            //((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ContactsSearchCriteriaFragment)}");
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Result.Ok && requestCode == AbstractCategoriesSearchView<SearchDocumentsCriteria>.RequestCodes.CategoriesRequest)
            {
                var ccsv = View.FindViewById<ContactCategoriesSearchView>(AbstractCategoriesSearchView<SearchContactsCriteria>.ViewId);
                if (ccsv != null)
                {
                    var categories = SerializationUtils.Deserialize<List<Category>>(data.Extras.GetString(PickCategoriesListActivity.CategoriesResultKey));
                    ccsv.SetSelectedCategoryIds(categories.Select(c => c.Id).ToList());
                }
            }
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
            return $"{nameof(ContactsSearchCriteriaFragment)}";
        }

        class ContactsSearchFragmentState : IRetainableState
        {

            public SearchContactsCriteria Criteria { get; set; }
        }
    }
}

