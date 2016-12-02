//
// Project: Mark5.Mobile.Droid
// File: MainActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Android.App.Activity]
    public class MainActivity : BaseAppCompatActivity, NavigationView.IOnNavigationItemSelectedListener, FragmentManager.IOnBackStackChangedListener
    {

        Toolbar toolbar;
        DrawerLayout drawer;
        SmoothActionBarDrawerToggle drawerToggle;
        NavigationView navigationView;
        AppCompatTextView navHeaderTitleTextView;
        AppCompatTextView navHeaderSubtitleTextView;
        IMenuItem lastSelectedItem;
        CoordinatorLayout coordinatorLayout;

        RetainedFragment<MainActivityState> stateFragment;

        bool permissionsAsked;

        #region Activity lifecycle

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Starting {nameof(MainActivity)}...");

            SetContentView(Resource.Layout.base_layout_nav);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            coordinatorLayout = FindViewById<CoordinatorLayout>(Resource.Id.coordinator);

            drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawerToggle = new SmoothActionBarDrawerToggle(this, drawer, Resource.String.open_drawer, Resource.String.close_drawer);
            drawer.AddDrawerListener(drawerToggle);
            drawerToggle.SyncState();

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            SupportFragmentManager.AddOnBackStackChangedListener(this);

            navigationView = FindViewById<NavigationView>(Resource.Id.navigation_view);
            navigationView.SetNavigationItemSelectedListener(this);

            var header = navigationView.GetHeaderView(0);
            navHeaderTitleTextView = header.FindViewById<AppCompatTextView>(Resource.Id.nav_header_title);
            navHeaderSubtitleTextView = header.FindViewById<AppCompatTextView>(Resource.Id.nav_header_subtitle);

            stateFragment = RetainedFragment<MainActivityState>.FindOrCreate(SupportFragmentManager, nameof(MainActivity));

            if (savedInstanceState == null)
            {
                stateFragment.State = new MainActivityState
                {
                    MenuItemContents = new Dictionary<int, MenuItemContent>
                    {
                        [Resource.Id.nav_search] = new SearchMenuItemContent(),
                        [Resource.Id.nav_documents] = new ModulesMenuItemContent(ModuleType.Documents),
                        [Resource.Id.nav_contacts] = new ModulesMenuItemContent(ModuleType.Contacts),
                        [Resource.Id.nav_shortcodes] = new ModulesMenuItemContent(ModuleType.Shortcodes),
                        [Resource.Id.nav_notifications] = new NotificationsMenuItemContent(),
                        [Resource.Id.nav_settings] = new PreferencesMenuItemContent()
                    }
                };

                var initialMenuItem = navigationView.Menu.FindItem(Resource.Id.nav_documents);
                initialMenuItem.SetChecked(true);
                OnNavigationItemSelected(initialMenuItem);

                Task.Run(async () =>
                {
                    var ci = await AuthenticatorFactory.Create().GetConnectionInfoAsync();
                    var ss = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);
                    return new { ConnectionInfo = ci, SystemSettings = ss };
                }).ContinueWith(t =>
                {
                    var ci = t.Result.ConnectionInfo;
                    var ss = t.Result.SystemSettings;

                    navHeaderTitleTextView.Text = $"{ss?.UserInfo?.User?.FirstName} {ss?.UserInfo?.User?.LastName}";
                    navHeaderSubtitleTextView.Text = $"{ci?.Username}@{ci?.Hostname}:{ci?.Port}";
                }, TaskScheduler.FromCurrentSynchronizationContext());

                CommonConfig.Logger.Info($"Created {nameof(MainActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(MainActivity)}");
            }
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);

            drawerToggle.DrawerIndicatorEnabled = SupportFragmentManager.BackStackEntryCount <= 1;
            drawerToggle.SyncState();
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (permissionsAsked)
                return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M && (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts) != Permission.Granted
                || ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted))
            {

                Action permissionRequestAction = () =>
                {
#pragma warning disable XA0001 // Find issues with Android API usage
                    RequestPermissions(new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.ReadContacts }, 769);
#pragma warning restore XA0001 // Find issues with Android API usage
                };

                var snackbar = Snackbar.Make(coordinatorLayout, Resource.String.permissions_snackbar_text, 10000)
                                       .SetAction(Resource.String.permissions_snackbar_action, v => permissionRequestAction());

                snackbar.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.brown));
                snackbar.View.SetBackgroundColor(new Android.Graphics.Color(ContextCompat.GetColor(this, Resource.Color.darkerblue)));
                snackbar.View.Clickable = true;
                snackbar.View.Click += (sender, e) =>
                {
                    permissionRequestAction();
                    snackbar.Dismiss();
                };
                snackbar.Show();
            }

            permissionsAsked = true;
        }

        public override void OnBackPressed()
        {
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else if (SupportFragmentManager.BackStackEntryCount > 1)
            {
                base.OnBackPressed();
            }
            else
            {
                Finish();
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            stateFragment.State.NavHeaderTitle = navHeaderTitleTextView.Text;
            stateFragment.State.NavHeaderSubtitle = navHeaderSubtitleTextView.Text;

            stateFragment.State.LastSelectedItemId = lastSelectedItem.ItemId;
            stateFragment.State.PermissionsAsked = permissionsAsked;
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            navHeaderTitleTextView.Text = stateFragment.State.NavHeaderTitle;
            navHeaderSubtitleTextView.Text = stateFragment.State.NavHeaderSubtitle;
            permissionsAsked = stateFragment.State.PermissionsAsked;

            var menuItemId = stateFragment.State.LastSelectedItemId;
            var menuItem = navigationView.Menu.FindItem(menuItemId);
            lastSelectedItem = menuItem;
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            drawerToggle.OnConfigurationChanged(newConfig);
        }

        #endregion

        #region Utility methods

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            CommonConfig.Logger.Info($"Switching to {menuItem.TitleFormatted}...");

            drawerToggle.RunWhenIdle(() =>
            {
                if (lastSelectedItem != menuItem)
                {
                    if (lastSelectedItem != null)
                    {
                        stateFragment.State.MenuItemContents[lastSelectedItem.ItemId].Save(SupportFragmentManager);
                    }

                    if (SupportFragmentManager.BackStackEntryCount > 0)
                    {
                        SupportFragmentManager.PopBackStackImmediate(SupportFragmentManager.GetBackStackEntryAt(0).Id, (int)Android.App.PopBackStackFlags.Inclusive);
                    }

                    stateFragment.State.MenuItemContents[menuItem.ItemId].RestoreOrCreate(SupportFragmentManager);

                    lastSelectedItem = menuItem;
                }
            }, lastSelectedItem == null);

            drawer.CloseDrawer(GravityCompat.Start);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public void OnBackStackChanged()
        {
            drawerToggle.DrawerIndicatorEnabled = SupportFragmentManager.BackStackEntryCount <= 1;
            drawerToggle.SyncState();
        }

        #endregion

        #region State class

        class MainActivityState
        {

            public string NavHeaderTitle { get; set; }

            public string NavHeaderSubtitle { get; set; }

            public int LastSelectedItemId { get; set; }

            public Dictionary<int, MenuItemContent> MenuItemContents { get; set; }

            public bool PermissionsAsked { get; set; }
        }

        #endregion

        #region MenuItemContent classes

        abstract class MenuItemContent
        {

            protected readonly List<Fragment.SavedState> BackstackStates = new List<Fragment.SavedState>();
            protected readonly List<string> SavedTags = new List<string>();

            public virtual void Save(FragmentManager fm) { }

            public virtual void RestoreOrCreate(FragmentManager fm) { }
        }

        class SearchMenuItemContent : MenuItemContent
        {

            public override void RestoreOrCreate(FragmentManager fm)
            {
                var ft = fm.BeginTransaction();
                var pf = new SearchFragment();
                ft.SetTransition(FragmentTransaction.TransitFragmentFade);
                ft.Replace(Resource.Id.fragment_container, pf, "SearchFragment");
                ft.AddToBackStack("SearchFragment");
                ft.Commit();
            }
        }

        class ModulesMenuItemContent : MenuItemContent
        {

            protected ModuleType ModuleType { get; private set; }

            public ModulesMenuItemContent(ModuleType moduleType)
            {
                ModuleType = moduleType;
            }

            public override void Save(FragmentManager fm)
            {
                BackstackStates.Clear();
                SavedTags.Clear();

                for (int i = 0; i < fm.BackStackEntryCount; i++)
                {
                    var tag = fm.GetBackStackEntryAt(i).Name;
                    var fragment = fm.FindFragmentByTag(tag);
                    var state = fm.SaveFragmentInstanceState(fragment);
                    SavedTags.Add(tag);
                    BackstackStates.Add(state);
                }
            }

            public override void RestoreOrCreate(FragmentManager fm)
            {
                if (BackstackStates == null || !BackstackStates.Any())
                {
                    var foldersListFragment = new FoldersListFragment
                    {
                        RemoteFolder = Folder.RootPerModule(ModuleType)
                    };

                    var tag = foldersListFragment.GenerateTag();
                    var ft = fm.BeginTransaction();
                    ft.SetTransition(FragmentTransaction.TransitFragmentFade);
                    ft.Replace(Resource.Id.fragment_container, foldersListFragment, tag);
                    ft.AddToBackStack(tag);
                    ft.Commit();
                }
                else
                {
                    var backStackStatesAndTags = BackstackStates.Zip(SavedTags, (state, tag) => new { State = state, Tag = tag });

                    foreach (var item in backStackStatesAndTags)
                    {
                        var state = item.State;
                        var tag = item.Tag;

                        var ft = fm.BeginTransaction();
                        var foldersListFragment = new FoldersListFragment();
                        foldersListFragment.SetInitialSavedState(state);
                        ft.SetTransition(FragmentTransaction.TransitFragmentFade);
                        ft.Replace(Resource.Id.fragment_container, foldersListFragment, tag);
                        ft.AddToBackStack(tag);
                        ft.Commit();
                    }

                    BackstackStates.Clear();
                    SavedTags.Clear();
                }
            }
        }

        class NotificationsMenuItemContent : MenuItemContent
        {

            public override void Save(FragmentManager fm)
            {
                BackstackStates.Clear();
                SavedTags.Clear();

                for (int i = 0; i < fm.BackStackEntryCount; i++)
                {
                    var tag = fm.GetBackStackEntryAt(i).Name;
                    var fragment = fm.FindFragmentByTag(tag);
                    var state = fm.SaveFragmentInstanceState(fragment);
                    SavedTags.Add(tag);
                    BackstackStates.Add(state);
                }
            }

            public override void RestoreOrCreate(FragmentManager fm)
            {
                if (BackstackStates == null || !BackstackStates.Any())
                {
                    var notificationsFragment = new NotificationsFragment();

                    var tag = notificationsFragment.GenerateTag();
                    var ft = fm.BeginTransaction();
                    ft.SetTransition(FragmentTransaction.TransitFragmentFade);
                    ft.Replace(Resource.Id.fragment_container, notificationsFragment, tag);
                    ft.AddToBackStack(tag);
                    ft.Commit();
                }
                else
                {
                    var backStackStatesAndTags = BackstackStates.Zip(SavedTags, (state, tag) => new { State = state, Tag = tag });

                    foreach (var item in backStackStatesAndTags)
                    {
                        var state = item.State;
                        var tag = item.Tag;

                        var ft = fm.BeginTransaction();
                        var notificationsFragment = new NotificationsFragment();
                        notificationsFragment.SetInitialSavedState(state);
                        ft.SetTransition(FragmentTransaction.TransitFragmentFade);
                        ft.Replace(Resource.Id.fragment_container, notificationsFragment, tag);
                        ft.AddToBackStack(tag);
                        ft.Commit();
                    }

                    BackstackStates.Clear();
                    SavedTags.Clear();
                }
            }
        }

        class PreferencesMenuItemContent : MenuItemContent
        {

            public override void RestoreOrCreate(FragmentManager fm)
            {
                var ft = fm.BeginTransaction();
                var pf = new PreferenceFragment();
                ft.SetTransition(FragmentTransaction.TransitFragmentFade);
                ft.Replace(Resource.Id.fragment_container, pf, "PreferenceFragment");
                ft.AddToBackStack("PreferenceFragment");
                ft.Commit();
            }
        }

        #endregion

    }
}

