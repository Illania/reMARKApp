//
// Project: Mark5.Mobile.Droid
// File: FoldersListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class FoldersListFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_list_folders, container, false);
        }

        public override void OnStart()
        {
            base.OnStart();

            var viewPager = View.FindViewById<ViewPager>(Resource.Id.folderPager);
            var adapter = new MyAdapter(Activity.SupportFragmentManager);
            viewPager.Adapter = adapter;

            var tabLayout = View.FindViewById<TabLayout>(Resource.Id.tab);
            tabLayout.SetupWithViewPager(viewPager);
        }

    }

    class MyAdapter : FragmentStatePagerAdapter
    {
        int numItems = 99;

        public MyAdapter(FragmentManager fm) : base(fm)
        {
        }

        public override int Count
        {
            get
            {
                return numItems;
            }
        }

        public override Fragment GetItem(int position)
        {
            var internalFragment = FoldersListInternalFragment.Create(position);
            return internalFragment;
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(position.ToString());
        }
    }
}

