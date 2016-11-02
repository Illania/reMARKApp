//
// Project: Mark5.Mobile.Droid
// File: SearchFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class SearchFragment : RetainableStateFragment
    {

        static readonly int[] tabTitles = { Resource.String.documents, Resource.String.contacts, Resource.String.shortcodes };

        TabLayout tabLayout;
        ViewPager pager;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(SearchFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.pager, container, false);

            tabLayout = rootView.FindViewById<TabLayout>(Resource.Id.tab_layout);

            pager = rootView.FindViewById<ViewPager>(Resource.Id.pager);
            pager.AddOnPageChangeListener(new TabLayout.TabLayoutOnPageChangeListener(tabLayout));
            pager.Adapter = new SearchPagerAdapter(Activity.SupportFragmentManager);

            tabLayout.TabSelected += (sender, e) => pager.CurrentItem = e.Tab.Position;

            for (var i = 0; i < 3; i++)
            {
                tabLayout.AddTab(tabLayout.NewTab().SetText(tabTitles[i]));
            }
            tabLayout.TabGravity = TabLayout.GravityFill;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = string.Empty;

            CommonConfig.Logger.Info($"Created {nameof(SearchFragment)}");
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new SearchRetainableState
            {
                SelectedTab = pager.CurrentItem
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var srs = restoredState as SearchRetainableState;
            if (srs != null)
            {
                pager.CurrentItem = srs.SelectedTab;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(SearchFragment)}";
        }

        class SearchRetainableState : IRetainableState
        {

            public int SelectedTab { get; set; }
        }

        class SearchPagerAdapter : FragmentPagerAdapter
        {

            public override int Count
            {
                get
                {
                    return 3;
                }
            }

            public override Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        return new DocumentsSearchCriteriaFragment();
                    case 1:
                        return new ContactsSearchCriteriaFragment();
                    case 2:
                        return new ShortcodesSearchCriteriaFragment();
                    default:
                        return null;
                }
            }

            public SearchPagerAdapter(FragmentManager fm)
                : base(fm)
            {
            }
        }
    }
}
