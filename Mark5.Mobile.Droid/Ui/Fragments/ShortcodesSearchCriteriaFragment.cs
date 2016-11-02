//
// Project: Mark5.Mobile.Droid
// File: ShortcodesSearchCriteriaFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
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

    public class ShortcodesSearchCriteriaFragment : RetainableStateFragment
    {

        LinearLayoutCompat linearLayout;
        AppCompatButton searchButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodesSearchCriteriaFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_button, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            searchButton = rootView.FindViewById<AppCompatButton>(Resource.Id.button);

            linearLayout.AddView(new ShortcodeNameSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ShortcodeDescriptionSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ShortcodeAddressSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ShortcodeFoldersSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new MaxShortcodesSearchView(Context));

            searchButton.Text = GetString(Resource.String.search);
            searchButton.Click += (sender, e) =>
            {
                var i = new Intent(Activity, typeof(SearchResultsActivity));
                i.PutExtra(SearchResultsActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Shortcodes));
                i.PutExtra(SearchResultsActivity.CriteriaIntentKey, SerializationUtils.Serialize(GetCriteria()));
                StartActivity(i);
            };

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            //((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search_shortcodes);

            CommonConfig.Logger.Info($"Created {nameof(ShortcodesSearchCriteriaFragment)}");
        }

        SearchShortcodesCriteria GetCriteria()
        {
            var criteria = new SearchShortcodesCriteria();

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                var dsv = linearLayout.GetChildAt(i) as AbstractSearchView<SearchShortcodesCriteria>;
                if (dsv != null)
                {
                    dsv.ToCriteria(criteria);
                }
            }

            return criteria;
        }

        void SetCriteria(SearchShortcodesCriteria criteria)
        {
            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                var dsv = linearLayout.GetChildAt(i) as AbstractSearchView<SearchShortcodesCriteria>;
                if (dsv != null)
                {
                    dsv.FromCriteria(criteria);
                }
            }
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new ShortcodesSearchFragmentState { Criteria = GetCriteria() };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dsfs = restoredState as ShortcodesSearchFragmentState;
            if (dsfs != null)
            {
                SetCriteria(dsfs.Criteria);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ShortcodesSearchCriteriaFragment)}";
        }

        class ShortcodesSearchFragmentState : IRetainableState
        {

            public SearchShortcodesCriteria Criteria { get; set; }
        }
    }
}

