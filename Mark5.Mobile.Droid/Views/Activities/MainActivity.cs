//
// Project: Mark5.Mobile.Droid
// File: MainActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Linq;
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
        const string StatesBundleString = "statesBundleString";

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

        Dictionary<ModuleType, List<Android.Support.V4.App.Fragment.SavedState>> statesForModule = new Dictionary<ModuleType, List<Android.Support.V4.App.Fragment.SavedState>>();

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            var newModuleType = GetModuleFromMenuId(menuItem.ItemId);

            if (lastSelectedItem != menuItem)
            {
                if (lastSelectedItem != null)
                {
                    var previousModuleType = GetModuleFromMenuId(lastSelectedItem.ItemId);
                    var states = new List<Android.Support.V4.App.Fragment.SavedState>();

                    var firstFragment = SupportFragmentManager.FindFragmentByTag(previousModuleType.ToString());
                    var firstFragmentState = SupportFragmentManager.SaveFragmentInstanceState(firstFragment);
                    states.Add(firstFragmentState);

                    for (int i = 0; i < SupportFragmentManager.BackStackEntryCount; i++)
                    {
                        var tag = SupportFragmentManager.GetBackStackEntryAt(i).Name;
                        var fragment = SupportFragmentManager.FindFragmentByTag(tag);
                        var state = SupportFragmentManager.SaveFragmentInstanceState(fragment);
                        states.Add(state);
                    }

                    statesForModule[previousModuleType] = states;
                }

                if (statesForModule.ContainsKey(newModuleType))
                {
                    ClearBackStack();

                    var ft = SupportFragmentManager.BeginTransaction();

                    var firstState = statesForModule[newModuleType][0];
                    var firstFoldersListFragment = FoldersListFragment.Create(SupportFragmentManager, newModuleType, null);
                    firstFoldersListFragment.SetInitialSavedState(firstState);
                    ft.Replace(Resource.Id.fragment_container, firstFoldersListFragment, newModuleType.ToString());
                    ft.Commit();

                    foreach (var state in statesForModule[newModuleType].Skip(1))
                    {
                        ft = SupportFragmentManager.BeginTransaction();
                        var foldersListFragment = FoldersListFragment.Create(SupportFragmentManager, newModuleType, null);
                        foldersListFragment.SetInitialSavedState(state);
                        var t = System.Guid.NewGuid().ToString();
                        ft.Replace(Resource.Id.fragment_container, foldersListFragment, t);
                        ft.AddToBackStack(t);
                        ft.Commit();
                    }

                    statesForModule[newModuleType].Clear();
                }
                else
                {
                    var foldersListFragment = FoldersListFragment.Create(SupportFragmentManager, newModuleType, null);

                    ClearBackStack();

                    var ft = SupportFragmentManager.BeginTransaction();
                    ft.Replace(Resource.Id.fragment_container, foldersListFragment, newModuleType.ToString());
                    ft.Commit();
                }

                lastSelectedItem = menuItem;
            }

            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        void ClearBackStack()
        {
            SupportFragmentManager.PopBackStackImmediate(null, (int)PopBackStackFlags.Inclusive);
        }

        ModuleType GetModuleFromMenuId(int itemId)
        {
            ModuleType moduleType = ModuleType.None;

            switch (itemId)
            {
                case Resource.Id.nav_documents:
                    moduleType = ModuleType.Documents;
                    break;
                case Resource.Id.nav_contacts:
                    moduleType = ModuleType.Contacts;
                    break;
                case Resource.Id.nav_shortcodes:
                    moduleType = ModuleType.Shortcodes;
                    break;
                case Resource.Id.nav_calendar:
                    moduleType = ModuleType.Calendar;
                    break;
                default:
                    break;
            }

            return moduleType;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutInt(MenuItemIdBundleString, lastSelectedItem.ItemId);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            var menuItemId = savedInstanceState.GetInt(MenuItemIdBundleString);
            var menuItem = navigationView.Menu.FindItem(menuItemId);
            lastSelectedItem = menuItem;
        }


        void FoldersListFragment.IFoldersListFragmentSelectedListener.NavigateInFolder(ModuleType moduleType, Folder folder)
        {
            var foldersListFragment = FoldersListFragment.Create(SupportFragmentManager, moduleType, folder);

            var ft = SupportFragmentManager.BeginTransaction();
            ft.SetTransition((int)FragmentTransit.FragmentOpen);
            ft.Replace(Resource.Id.fragment_container, foldersListFragment, folder.Id.ToString()); //TODO need to decide on a common tag
            ft.AddToBackStack(folder.Id.ToString());
            ft.Commit();
        }

        void FoldersListFragment.IFoldersListFragmentSelectedListener.SetTitles(string title, string subtitle)
        {
            SupportActionBar.Title = title;
            SupportActionBar.Subtitle = subtitle;
        }
    }

}

