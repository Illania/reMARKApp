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

        const string RetainStateFragmentTag = "MainActivity_RetainStateFragmentTag";

        RetainedFragment<MainActivityState> stateFragment;

        public Dictionary<int, MenuItemContent> menuItemContents
        {
            get
            {
                return stateFragment.State.MenuItemContents;
            }
        }

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

            stateFragment = RetainedFragment<MainActivityState>.FindOrCreate(SupportFragmentManager, RetainStateFragmentTag);

            if (savedInstanceState == null)
            {
                var mainActivityState = new MainActivityState();

                mainActivityState.MenuItemContents = new Dictionary<int, MenuItemContent>()
                {
                    {Resource.Id.nav_documents,  new DocumentsModuleMenuItemContent()},
                    {Resource.Id.nav_contacts,  new ContactsModuleMenuItemContent()},
                    {Resource.Id.nav_shortcodes,  new ShortcodesModuleMenuItemContent()},
                    {Resource.Id.nav_calendar,  new CalendarModuleMenuItemContent()},
                };

                stateFragment.State = mainActivityState;

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
                    var headerTitle = FindViewById<AppCompatTextView>(Resource.Id.nav_header_title);
                    var headerSubtitle = FindViewById<AppCompatTextView>(Resource.Id.nav_header_subtitle);

                    headerTitle.Text = $"{ss?.UserInfo?.User?.FirstName} {ss?.UserInfo?.User?.LastName}";

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
            else if (SupportFragmentManager.BackStackEntryCount > 1)
            {
                base.OnBackPressed();
            }
            else
            {
                Finish();
            }
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            if (lastSelectedItem != menuItem)
            {
                if (lastSelectedItem != null)
                {
                    menuItemContents[lastSelectedItem.ItemId].Save(SupportFragmentManager);
                }

                ClearBackStack();

                menuItemContents[menuItem.ItemId].RestoreOrCreate(SupportFragmentManager);

                lastSelectedItem = menuItem;
            }

            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        void ClearBackStack()
        {
            if (SupportFragmentManager.BackStackEntryCount > 0)
            {
                SupportFragmentManager.PopBackStackImmediate(SupportFragmentManager.GetBackStackEntryAt(0).Id, (int)PopBackStackFlags.Inclusive);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            stateFragment.State.LastSelectedItemId = lastSelectedItem.ItemId;
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            var menuItemId = stateFragment.State.LastSelectedItemId; //TODO check if it can be done in a more clever way
            var menuItem = navigationView.Menu.FindItem(menuItemId);
            lastSelectedItem = menuItem;
        }

        void FoldersListFragment.IFoldersListFragmentSelectedListener.NavigateInFolder(ModuleType moduleType, Folder folder)
        {
            var foldersListFragment = new FoldersListFragment
            {
                CurrentFolder = folder,
                ModuleType = moduleType,
            };

            var tag = foldersListFragment.GenerateTag();
            var ft = SupportFragmentManager.BeginTransaction();
            ft.SetTransition((int)FragmentTransit.FragmentOpen);
            ft.Replace(Resource.Id.fragment_container, foldersListFragment, tag);
            ft.AddToBackStack(tag);
            ft.Commit();
        }

        void FoldersListFragment.IFoldersListFragmentSelectedListener.SetTitles(string title, string subtitle)
        {
            SupportActionBar.Title = title;
            SupportActionBar.Subtitle = subtitle;
        }
    }

    public abstract class MenuItemContent
    {
        protected readonly List<Android.Support.V4.App.Fragment.SavedState> backstackStates = new List<Android.Support.V4.App.Fragment.SavedState>();
        protected readonly List<string> savedTags = new List<string>();

        public abstract void Save(Android.Support.V4.App.FragmentManager fm);
        public abstract void RestoreOrCreate(Android.Support.V4.App.FragmentManager fm);
    }

    public abstract class ModulesMenuItemContent : MenuItemContent
    {
        protected abstract ModuleType ModuleType { get; }

        public override void Save(Android.Support.V4.App.FragmentManager fm)
        {
            backstackStates.Clear();

            for (int i = 0; i < fm.BackStackEntryCount; i++)
            {
                var tag = fm.GetBackStackEntryAt(i).Name;
                savedTags.Add(tag);
                var fragment = fm.FindFragmentByTag(tag);
                var state = fm.SaveFragmentInstanceState(fragment);
                backstackStates.Add(state);
            }
        }

        public override void RestoreOrCreate(Android.Support.V4.App.FragmentManager fm)
        {
            if (backstackStates == null || !backstackStates.Any())
            {
                //Create 
                var foldersListFragment = new FoldersListFragment
                {
                    CurrentFolder = Folder.RootPerModule(ModuleType),
                    ModuleType = ModuleType,
                };

                var tag = foldersListFragment.GenerateTag();
                var ft = fm.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, foldersListFragment, tag);
                ft.AddToBackStack(tag);

                ft.Commit();
            }
            else
            {
                //Restore
                var index = 0;
                var backStackStatesAndTags = backstackStates.Zip(savedTags, (state, tag) => new { State = state, Tag = tag });

                foreach (var item in backStackStatesAndTags)
                {
                    var state = item.State;
                    var tag = item.Tag;

                    var ft = fm.BeginTransaction();
                    var foldersListFragment = new FoldersListFragment();
                    foldersListFragment.SetInitialSavedState(state);
                    ft.Replace(Resource.Id.fragment_container, foldersListFragment, tag);
                    ft.AddToBackStack(tag);
                    ft.Commit();
                    index++;
                }

                backstackStates.Clear();
                savedTags.Clear();
            }
        }
    }

    public class MainActivityState
    {
        public int LastSelectedItemId
        {
            get;
            set;
        }

        public Dictionary<int, MenuItemContent> MenuItemContents
        {
            get;
            set;
        }
    }

    public class DocumentsModuleMenuItemContent : ModulesMenuItemContent
    {
        protected override ModuleType ModuleType
        {
            get { return ModuleType.Documents; }
        }
    }

    public class ContactsModuleMenuItemContent : ModulesMenuItemContent
    {
        protected override ModuleType ModuleType
        {
            get { return ModuleType.Contacts; }
        }
    }

    public class ShortcodesModuleMenuItemContent : ModulesMenuItemContent
    {
        protected override ModuleType ModuleType
        {
            get { return ModuleType.Shortcodes; }
        }
    }

    public class CalendarModuleMenuItemContent : ModulesMenuItemContent
    {
        protected override ModuleType ModuleType
        {
            get { return ModuleType.Calendar; }
        }
    }
}

