using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.Tabs;
using AndroidX.Fragment.App;
using AndroidX.AppCompat.App;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class CopyToWorktrayFragment : BaseFragment, ViewPager.IOnPageChangeListener
    {
        static readonly int[] tabTitles =
        {
            Resource.String.users,
            Resource.String.departments
        };

        const string SelectedTabKey = "SelectedTab_8224d3cf-48b2-48d4-92e9-c36ecc8476bb";
        const string IdsIntentKey = "IdsIntentKey";
        const string ObjectTypeIntentKey = "ObjectTypeIntentKey";
        const string DelayedCopyBundleKey = "DelayedCopy_ed011f46-e180-462c-9d49-5dc047f3c325";

        List<int> businessEntitiesIds;
        ObjectType objectType;
        bool delayedCopy; 

        Folder remoteFolder;

        TabLayout tabLayout;
        ViewPager pager;

        int savedCurrentPageIndex = -1;

        public static CopyToWorktrayFragment NewInstance()
        {
            return new CopyToWorktrayFragment();
        }

        public static (CopyToWorktrayFragment fragment, string tag) NewInstance(List<int> ids, ObjectType ot, bool? delayedCopy = false)
        {
            var args = new Bundle();

            if (ids != null)
                args.PutString(IdsIntentKey, Serializer.Serialize(ids));

            args.PutInt(ObjectTypeIntentKey, (int)ot);

            if (delayedCopy != null)
                args.PutBoolean(DelayedCopyBundleKey, delayedCopy.Value);

            var fragment = new CopyToWorktrayFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(CopyToWorktrayFragment)}]";

            return (fragment, tag);
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(IdsIntentKey))
                businessEntitiesIds = Serializer.Deserialize<List<int>>(Arguments.GetString(IdsIntentKey));

            objectType = (ObjectType)Arguments.GetInt(ObjectTypeIntentKey);

            if (Arguments.ContainsKey(DelayedCopyBundleKey))
                delayedCopy = Arguments.GetBoolean(DelayedCopyBundleKey);

            if (savedInstanceState?.ContainsKey(SelectedTabKey) == true)
                savedCurrentPageIndex = savedInstanceState.GetInt(SelectedTabKey);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(FoldersNotificationsListFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.pager, container, false);

            tabLayout = rootView.FindViewById<TabLayout>(Resource.Id.tab_layout);

            pager = rootView.FindViewById<ViewPager>(Resource.Id.pager);
            pager.OffscreenPageLimit = 1;
            pager.AddOnPageChangeListener(new TabLayout.TabLayoutOnPageChangeListener(tabLayout));
            pager.AddOnPageChangeListener(this);
            pager.Adapter = new PagerAdapter(ChildFragmentManager, businessEntitiesIds, objectType, delayedCopy);


            tabLayout.TabSelected += (sender, e) => pager.CurrentItem = e.Tab.Position;

            for (var i = 0; i < 2; i++)
                tabLayout.AddTab(tabLayout.NewTab().SetText(tabTitles[i]));

            tabLayout.TabGravity = TabLayout.GravityFill;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (savedCurrentPageIndex >= 0)
                pager.CurrentItem = savedCurrentPageIndex;


            var title = GetString(Resource.String.copy_to_worktray);


            ((AppCompatActivity)Activity).SupportActionBar.Title = title;

            CommonConfig.Logger.Info($"Created {nameof(CopyToWorktrayFragment)}");
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (pager != null || savedCurrentPageIndex >= 0)
                outState.PutInt(SelectedTabKey, pager?.CurrentItem ?? savedCurrentPageIndex);
        }

        void ViewPager.IOnPageChangeListener.OnPageScrollStateChanged(int state)
        {
            //Nothing to do...
        }

        void ViewPager.IOnPageChangeListener.OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
            //Nothing to do...
        }

        void ViewPager.IOnPageChangeListener.OnPageSelected(int position)
        {
            if (position == 1)
                CommonConfig.UsageAnalytics.LogEvent(new OpenNotificationsEvent(ModuleType.Documents));
        }

        class PagerAdapter : FragmentPagerAdapter
        {
            public override int Count => 2;

            List<int> businessEntitiesIds;
            ObjectType objectType;
            bool delayedCopy;

            public PagerAdapter(FragmentManager fm, List<int> businessEntitiesIds,  ObjectType objectType, bool delayedCopy)
                : base(fm)
            {
                this.objectType = objectType;
                this.businessEntitiesIds = businessEntitiesIds;
                this.delayedCopy = delayedCopy;
            }

            public override Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        //Item1 is the fragment
                        return CopyToUserWorktrayFragment.NewInstance(businessEntitiesIds,objectType,delayedCopy).fragment;
                    case 1:
                        return CopyToDepartmentWorktrayFragment.NewInstance(businessEntitiesIds,objectType, delayedCopy).fragment;
                    default:
                        return null;
                }
            }
        }
    }


}