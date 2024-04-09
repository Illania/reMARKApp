using Android.OS;
using Android.Views;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Droid.Ui.Common;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.Tabs;
using AndroidX.Fragment.App;
using AndroidX.AppCompat.App;
using View = Android.Views.View;
using Switch = AndroidX.AppCompat.Widget.SwitchCompat;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class DocumentsUnreadDocumentsListFragment : BaseFragment, ViewPager.IOnPageChangeListener
    {
        static readonly int[] tabTitles =
        {
            Resource.String.documents,
            Resource.String.unread_documents
        };

        const string RemoteFolderBundleKey = "RemoteFolder_403cbd64-83e9-4e13-8809-0868debb55b9";
        const string SelectedTabKey = "SelectedTab_8224d3cf-48b2-48d4-92e9-c36ecc84760a";

        Folder remoteFolder;

        TabLayout tabLayout;
        ViewPager pager;

        IMenu menu;
        Switch switchControl;

        int savedCurrentPageIndex = -1;

        public static DocumentsUnreadDocumentsListFragment NewInstance()
        {
            return new DocumentsUnreadDocumentsListFragment();
        }

        public static (DocumentsUnreadDocumentsListFragment fragment, string tag) NewInstance(Folder remoteFolder)
        {
            var args = new Bundle();

            if (remoteFolder != null)
                args.PutString(RemoteFolderBundleKey, Serializer.Serialize(remoteFolder.ShallowCopy()));

            var fragment = new DocumentsUnreadDocumentsListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(FoldersNotificationsListFragment)} [FolderId={remoteFolder.Id}, ModuleType={remoteFolder.Module}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);     

            if (Arguments?.ContainsKey(RemoteFolderBundleKey) == true)
                remoteFolder = Serializer.Deserialize<Folder>(Arguments.GetString(RemoteFolderBundleKey));

            if (savedInstanceState?.ContainsKey(SelectedTabKey) == true)
                savedCurrentPageIndex = savedInstanceState.GetInt(SelectedTabKey);      
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(FoldersNotificationsListFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.pager_documentlist, container, false);

            tabLayout = rootView.FindViewById<TabLayout>(Resource.Id.tab_layout);

            pager = rootView.FindViewById<ViewPager>(Resource.Id.pager);
            pager.OffscreenPageLimit = 1;
            pager.AddOnPageChangeListener(new TabLayout.TabLayoutOnPageChangeListener(tabLayout));
            pager.AddOnPageChangeListener(this);
            pager.Adapter = new PagerAdapter(ChildFragmentManager, remoteFolder);

            tabLayout.TabSelected += (sender, e) => pager.CurrentItem = e.Tab.Position;

            for (var i = 0; i < 2; i++)
                tabLayout.AddTab(tabLayout.NewTab().SetText(tabTitles[i]));

            tabLayout.TabGravity = TabLayout.GravityFill;

            switchControl = rootView.FindViewById<Switch>(Resource.Id.unread_switch);
            switchControl.CheckedChange += (sender, e) =>
            {
                if (switchControl.Checked)
                {
                    pager.CurrentItem = 1;
                }
                else
                {
                     pager.CurrentItem = 0;
                }
            };

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (savedCurrentPageIndex >= 0)
                pager.CurrentItem = savedCurrentPageIndex;

            if (remoteFolder.Root)
                remoteFolder = Folder.RootForModule(remoteFolder.Module);

            var title = string.Empty;

            switch (remoteFolder.Module)
            {
                case ModuleType.Documents:
                    title = GetString(Resource.String.documents);
                    break;
                case ModuleType.Contacts:
                    title = GetString(Resource.String.contacts);
                    break;
                case ModuleType.Shortcodes:
                    title = GetString(Resource.String.shortcodes);
                    break;
            }

            ((AppCompatActivity)Activity).SupportActionBar.Title = title;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = remoteFolder.Root ? null : remoteFolder.Name;

            CommonConfig.Logger.Info($"Created {nameof(FoldersNotificationsListFragment)}");
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

            readonly Folder folder;

            public PagerAdapter(FragmentManager fm, Folder folder) : base(fm)
            {
                this.folder = folder;
            }

            public override Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        //Item1 is the fragment
                        return DocumentsListFragment.NewInstance(folder).fragment;
                        //return FoldersListFragment.NewInstance(folder, loadRemoteFromCache: folder.SubFolders?.Any()).fragment;
                    case 1:
                        return UnreadDocumentsListFragment.NewInstance(folder).fragment;
                    default:
                        return null;
                }
            }
        }
    }
}
