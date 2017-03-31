//
// Project: Mark5.Mobile.Droid
// File: FoldersNotificationsListFragment.cs
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
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class FoldersNotificationsListFragment : RetainableStateFragment
    {

        static readonly int[] tabTitles = { Resource.String.folders, Resource.String.notifications };

        public Folder RemoteFolder { get; set; }

        TabLayout tabLayout;
        ViewPager pager;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(FoldersNotificationsRetainableState)}...");

            var rootView = inflater.Inflate(Resource.Layout.pager, container, false);

            tabLayout = rootView.FindViewById<TabLayout>(Resource.Id.tab_layout);

            pager = rootView.FindViewById<ViewPager>(Resource.Id.pager);
            pager.OffscreenPageLimit = 1;
            pager.AddOnPageChangeListener(new TabLayout.TabLayoutOnPageChangeListener(tabLayout));
            pager.Adapter = new PagerAdapter(ChildFragmentManager, RemoteFolder);

            tabLayout.TabSelected += (sender, e) => pager.CurrentItem = e.Tab.Position;

            for (var i = 0; i < 2; i++)
            {
                tabLayout.AddTab(tabLayout.NewTab().SetText(tabTitles[i]));
            }
            tabLayout.TabGravity = TabLayout.GravityFill;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = RemoteFolder.Module.ToString();
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = RemoteFolder.Root ? null : RemoteFolder.Name;

            CommonConfig.Logger.Info($"Created {nameof(FoldersNotificationsRetainableState)}");
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new FoldersNotificationsRetainableState
            {
                Folder = RemoteFolder,
                SelectedTab = pager.CurrentItem
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var srs = restoredState as FoldersNotificationsRetainableState;
            if (srs != null)
            {
                RemoteFolder = srs.Folder;
                pager.CurrentItem = srs.SelectedTab;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(FoldersNotificationsListFragment)} [FolderId={RemoteFolder?.Id}, ModuleType={RemoteFolder?.Module}]";
        }

        class FoldersNotificationsRetainableState : IRetainableState
        {

            public Folder Folder { get; set; }

            public int SelectedTab { get; set; }
        }

        class PagerAdapter : FragmentPagerAdapter
        {

            public override int Count { get { return 2; } }

            readonly Folder folder;

            public PagerAdapter(FragmentManager fm, Folder folder)
                : base(fm)
            {
                this.folder = folder;
            }

            public override Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        return new FoldersListFragment { RemoteFolder = folder };
                    case 1:
                        return new NotificationsListFragment { ObjectTypes = folder.Module.ObjectTypes() };
                    default:
                        return null;
                }
            }
        }
    }
}
