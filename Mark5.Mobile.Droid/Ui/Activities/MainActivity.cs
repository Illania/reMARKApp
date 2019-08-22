using System;
using System.Collections.Generic;
using System.Linq;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Coordinators;
using Mark5.Mobile.Droid.Ui.Fragments;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Android.App.Activity]
    public class MainActivity : BaseAppCompatActivity, NavigationView.IOnNavigationItemSelectedListener, FragmentManager.IOnBackStackChangedListener
    {
        const string StateKey = "State_d7a09340-3478-43d7-93c3-8974b687a5ec";

        Toolbar toolbar;
        DrawerLayout drawer;
        SmoothActionBarDrawerToggle drawerToggle;
        NavigationView navigationView;
        AppCompatImageView navHeaderSettingsButton;
        AppCompatTextView navHeaderTitleTextView;
        IMenuItem lastSelectedItem;
        CoordinatorLayout coordinatorLayout;

        public CalendarModuleCoordinator CalendarCoordinator;

        bool firstSelection = true;
        bool permissionsAsked;

        MainActivityState state;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(MainActivity));
        }

        #region Activity lifecycle
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Starting {nameof(MainActivity)}...");

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("OTQ4NDdAMzEzNzJlMzEyZTMwWTh6RmRibklqNHU3citRNzViZVRJUkVkYmFSZTc1dFBEQi9td2dZSVZHWT0=");

            OverridePendingTransition(Resource.Animation.fade_in, Resource.Animation.fade_out);

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

            navHeaderSettingsButton = header.FindViewById<AppCompatImageView>(Resource.Id.nav_header_settings_button);
            navHeaderSettingsButton.Clickable = true;
            navHeaderSettingsButton.Click += NavHeaderSettingsButton_Click;

            navHeaderTitleTextView = header.FindViewById<AppCompatTextView>(Resource.Id.nav_header_title);

            CalendarCoordinator = new CalendarModuleCoordinator(this);

            if (savedInstanceState == null)
            {
                state = new MainActivityState
                {
                    MenuItemContents = new Dictionary<int, MenuItemContent>
                    {
                        [Resource.Id.nav_calendar] = new MenuItemContent(ModuleType.Calendar, CalendarCoordinator),
                        [Resource.Id.nav_contacts] = new MenuItemContent(ModuleType.Contacts),
                        [Resource.Id.nav_shortcodes] = new MenuItemContent(ModuleType.Shortcodes),
                        [Resource.Id.nav_documents] = new MenuItemContent(ModuleType.Documents), //TEsting
                    }
                };

                var initialMenuItem = navigationView.Menu.FindItem(Resource.Id.nav_calendar);
                initialMenuItem.SetChecked(true);
                OnNavigationItemSelected(initialMenuItem);

                var ss = AsyncHelpers.RunSync(() => Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local));
                navHeaderTitleTextView.Text = $"{ss?.UserInfo?.User?.FirstName} {ss?.UserInfo?.User?.LastName}";

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

            OnBoardingUtilities.ShowOnBoardingIfNecessary(this);

            if (permissionsAsked)
                return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M && (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts) != Permission.Granted || ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted
                                                                || ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadPhoneState) != Permission.Granted))
            {
                Action permissionRequestAction = () =>
                {
#pragma warning disable XA0001 // Find issues with Android API usage
                    RequestPermissions(new string[]
                        {
                            Manifest.Permission.ReadExternalStorage,
                            Manifest.Permission.ReadContacts,
                            Manifest.Permission.ReadPhoneState
                        },
                        769);
#pragma warning restore XA0001 // Find issues with Android API usage
                };


                var snackbar = Snackbar.Make(coordinatorLayout, Resource.String.permissions_snackbar_text, Snackbar.LengthIndefinite).SetAction(Resource.String.permissions_snackbar_action, v => permissionRequestAction());

                snackbar.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.lightblue));
                snackbar.View.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkblue)));
                snackbar.View.Clickable = true;
                snackbar.View.Click += (sender, e) =>
                {
                    permissionRequestAction();
                    snackbar.Dismiss();
                };
                snackbar.Show();
            }

            permissionsAsked = true;

            CheckAutoSavedDocument();
        }

        public override void OnBackPressed()
        {
            if (drawer.IsDrawerOpen(GravityCompat.Start))
                drawer.CloseDrawer(GravityCompat.Start);
            else if (SupportFragmentManager.BackStackEntryCount > 1)
                base.OnBackPressed();
            else
                Finish();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            state.NavHeaderTitle = navHeaderTitleTextView.Text;

            state.LastSelectedItemId = lastSelectedItem.ItemId;
            state.PermissionsAsked = permissionsAsked;

            outState.PutString(StateKey, Serializer.Serialize(state));
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            if (savedInstanceState?.ContainsKey(StateKey) == true)
            {
                state = Serializer.Deserialize<MainActivityState>(savedInstanceState.GetString(StateKey));
                navHeaderTitleTextView.Text = state.NavHeaderTitle;
                permissionsAsked = state.PermissionsAsked;

                var menuItemId = state.LastSelectedItemId;
                var menuItem = navigationView.Menu.FindItem(menuItemId);
                lastSelectedItem = menuItem;
            }
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            drawerToggle.OnConfigurationChanged(newConfig);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (drawerToggle.OnOptionsItemSelected(item))
                return true;

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Utility methods

        public void LockDrawer()
        {
            drawer?.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
        }

        public void UnlockDrawer()
        {
            drawer?.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            CommonConfig.Logger.Info($"Switching to {menuItem.TitleFormatted}...");
            drawerToggle.RunWhenIdle(() =>
                {
                    if (lastSelectedItem != menuItem)
                    {
                        if (lastSelectedItem != null)
                            state.MenuItemContents[lastSelectedItem.ItemId].Save(SupportFragmentManager);

                        if (SupportFragmentManager.BackStackEntryCount > 0)
                            SupportFragmentManager.PopBackStackImmediate(SupportFragmentManager.GetBackStackEntryAt(0).Id, (int)Android.App.PopBackStackFlags.Inclusive);

                        if (firstSelection)
                            firstSelection = false;
                        else
                            CommonConfig.UsageAnalytics.LogEvent(new OpenModuleEvent(state.MenuItemContents[menuItem.ItemId].ModuleType));

                        state.MenuItemContents[menuItem.ItemId].CreateOrRestore(SupportFragmentManager);

                        lastSelectedItem = menuItem;
                    }
                },
                lastSelectedItem == null);

            drawer.CloseDrawer(GravityCompat.Start);

            return true;
        }

        public void OnBackStackChanged()
        {
            drawerToggle.DrawerIndicatorEnabled = SupportFragmentManager.BackStackEntryCount <= 1;
            drawerToggle.SyncState();
        }

        void NavHeaderSettingsButton_Click(object sender, EventArgs e)
        {
            drawerToggle.RunWhenIdle(() =>
            {
                StartActivity(PreferenceActivity.CreateIntent(this));
            });

            drawer.CloseDrawer(GravityCompat.Start);
        }

        async void CheckAutoSavedDocument()
        {
            try
            {
                var isAvailable = await Managers.DocumentsManager.IsDocumentWorkingCopyAvailableAsync();
                if (!isAvailable)
                    return;

                var shouldRecover = await Dialogs.ShowYesNoDialogAsync(this, Resource.String.autosave_recover_title, Resource.String.autosave_recover_content);
                if (shouldRecover)
                {
                    CommonConfig.UsageAnalytics.LogEvent(new DocumentRecoveredEvent(true));
                    StartActivity(ComposeDocumentActivity.CreateIntent(this, DocumentCreationModeFlag.None, CopyToNewOption.None, true));
                }
                else
                {
                    CommonConfig.UsageAnalytics.LogEvent(new DocumentRecoveredEvent(false));
                    await Managers.DocumentsManager.DeleteDocumentWorkingCopyAsync();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while checking if there is an autosaved document", ex);
            }
        }

        #endregion

        #region State class

        class MainActivityState
        {
            public string NavHeaderTitle { get; set; }

            public int LastSelectedItemId { get; set; }

            public Dictionary<int, MenuItemContent> MenuItemContents { get; set; }

            public bool PermissionsAsked { get; set; }
        }

        #endregion

        #region MenuItemContent classes

        class MenuItemContent
        {
            protected readonly List<Fragment.SavedState> BackstackStates = new List<Fragment.SavedState>();
            protected readonly List<Bundle> Arguments = new List<Bundle>();
            protected readonly List<string> SavedTags = new List<string>();

            CalendarModuleCoordinator coordinator;

            public ModuleType ModuleType { get; }

            public MenuItemContent(ModuleType moduleType, CalendarModuleCoordinator cal = null) //TODO this is so shitty
            {
                ModuleType = moduleType;
                coordinator = cal;
            }

            public void Save(FragmentManager fm)
            {
                Arguments.Clear();
                BackstackStates.Clear();
                SavedTags.Clear();

                for (var i = 0; i < fm.BackStackEntryCount; i++)
                {
                    var tag = fm.GetBackStackEntryAt(i).Name;
                    var fragment = fm.FindFragmentByTag(tag);
                    var state = fm.SaveFragmentInstanceState(fragment);
                    SavedTags.Add(tag);
                    BackstackStates.Add(state);
                    Arguments.Add(fragment.Arguments);
                }
            }

            public void CreateOrRestore(FragmentManager fm)
            {
                if (BackstackStates == null || !BackstackStates.Any())
                    Create(fm);
                else
                    Restore(fm);
            }


            void Create(FragmentManager fm)
            {
                BaseFragment f;
                string tag;

                if (ModuleType == ModuleType.Calendar)
                    (f, tag) = coordinator.GetMainFragment();
                else if (ModuleType == ModuleType.Documents)
                    (f, tag) = FoldersNotificationsListFragment.NewInstance(Folder.RootForModule(ModuleType));
                else //(ModuleType == ModuleType.Contacts || ModuleType == ModuleType.Shortcodes || ModuleType == ModuleType.Calendar)
                    (f, tag) = FoldersListFragment.NewInstance(Folder.RootForModule(ModuleType));

                var ft = fm.BeginTransaction();
                ft.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out);
                ft.Replace(Resource.Id.fragment_container, f, tag);
                ft.AddToBackStack(tag);
                ft.Commit();
            }

            void Restore(FragmentManager fm)
            {
                for (int i = 0; i < BackstackStates.Count; i++)
                {
                    var state = BackstackStates[i];
                    var tag = SavedTags[i];
                    var arguments = Arguments[i];

                    BaseFragment f = null;

                    if (ModuleType == ModuleType.Calendar)
                        (f, tag) = coordinator.GetMainFragment();
                    else if (tag.StartsWith(nameof(FoldersNotificationsListFragment), StringComparison.Ordinal))
                        f = FoldersNotificationsListFragment.NewInstance();
                    else if (tag.StartsWith(nameof(FoldersListFragment), StringComparison.Ordinal))
                        f = FoldersListFragment.NewInstance();

                    f.Arguments = arguments;
                    f.SetInitialSavedState(state);

                    var ft = fm.BeginTransaction();
                    ft.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out);
                    ft.Replace(Resource.Id.fragment_container, f, tag);
                    ft.AddToBackStack(tag);
                    ft.Commit();
                }

                BackstackStates.Clear();
                SavedTags.Clear();
                Arguments.Clear();
            }
        }

        #endregion
    }
}