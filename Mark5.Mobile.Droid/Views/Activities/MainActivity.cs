//
// Project: Mark5.Mobile.Droid
// File: MainActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Views.Common;
using Mark5.Mobile.Droid.Views.Fragments;

namespace Mark5.Mobile.Droid.Views.Activity
{

    [Activity]
    public class MainActivity : BaseAppCompatActivity, NavigationView.IOnNavigationItemSelectedListener, FoldersListFragment.IFoldersListFragmentSelectedListener
    {
        Toolbar toolbar;
        DrawerLayout drawer;
        ActionBarDrawerToggle drawerToggle;
        NavigationView navigationView;

        IMenuItem lastSelectedItem;

        const string MenuItemIdBundleString = "menuItemId";
        const string ActionBarTitleBundleString = "actionBarTitleBundleString";
        const string ActionBarSubtitleBundleString = "actionBarSubtitleBundleString";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawerToggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);
            drawer.AddDrawerListener(drawerToggle);
            drawerToggle.SyncState();

            navigationView = FindViewById<NavigationView>(Resource.Id.navigation_view);
            navigationView.SetNavigationItemSelectedListener(this);

            if (savedInstanceState == null)
            {
                var initialMenuItem = navigationView.Menu.FindItem(Resource.Id.nav_documents);
                initialMenuItem.SetChecked(true);
                OnNavigationItemSelected(initialMenuItem);
            }

            Task.Run(async () =>
            {
                var authenticator = AuthenticatorFactory.Create();
                var ci = await authenticator.GetConnectionInfoAsync();
                var ss = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);

                RunOnUiThreadIfNecessary(() =>
                {
                    var headerTitle = FindViewById<AppCompatTextView>(Resource.Id.nav_header_title); //TODO sometimes the header title is null
                    var headerSubtitle = FindViewById<AppCompatTextView>(Resource.Id.nav_header_subtitle);

                    headerTitle.Text = $"{ss?.UserInfo?.User?.FirstName} {ss?.UserInfo?.User?.LastName}"; //TODO this should probably be passed as a bundle or something

                    headerSubtitle.Text = $"{ci?.Username}@{ci?.Hostname}:{ci?.Port}";
                });
            });
        }

        public override void OnBackPressed()
        {
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            if (lastSelectedItem != menuItem)
            {
                lastSelectedItem = menuItem;

                var foldersListFragment = FoldersListFragment.Create(ModuleType.Documents, null);

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, foldersListFragment, "0");
                ft.Commit();
            }

            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutInt(MenuItemIdBundleString, lastSelectedItem.ItemId);
            outState.PutString(ActionBarTitleBundleString, SupportActionBar.Title); //TODO need to investigate why the title is not saved after orientation changes
            outState.PutString(ActionBarSubtitleBundleString, SupportActionBar.Subtitle);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            var menuItemId = savedInstanceState.GetInt(MenuItemIdBundleString);
            var menuItem = navigationView.Menu.FindItem(menuItemId);
            lastSelectedItem = menuItem;

            SupportActionBar.Title = savedInstanceState.GetString(ActionBarTitleBundleString);
            SupportActionBar.Subtitle = savedInstanceState.GetString(ActionBarSubtitleBundleString);
        }

        void FoldersListFragment.IFoldersListFragmentSelectedListener.NavigateInFolder(ModuleType moduleType, Folder folder)
        {
            var foldersListFragment = FoldersListFragment.Create(moduleType, folder);

            var ft = SupportFragmentManager.BeginTransaction();
            ft.SetTransition((int)FragmentTransit.FragmentOpen);
            ft.Replace(Resource.Id.fragment_container, foldersListFragment, folder.Id.ToString());
            ft.AddToBackStack(null);
            ft.Commit();
        }

        void FoldersListFragment.IFoldersListFragmentSelectedListener.SetTitles(string title, string subtitle)
        {
            SupportActionBar.Title = title;
            SupportActionBar.Subtitle = subtitle;
        }
    }
}

