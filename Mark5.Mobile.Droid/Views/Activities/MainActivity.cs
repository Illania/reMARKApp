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
using HockeyApp.Android;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities.Hockey;
using Mark5.Mobile.Droid.Views.Common;
using Mark5.Mobile.Droid.Views.Fragments;

namespace Mark5.Mobile.Droid.Views.Activity
{

    [Activity]
    public class MainActivity : BaseAppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {

        Toolbar toolbar;
        DrawerLayout drawer;
        ActionBarDrawerToggle drawerToggle;
        NavigationView navigationView;

        IMenuItem lastSelectedItem;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetTitle(Resource.String.app_name);
            SetContentView(Resource.Layout.activity_main);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawerToggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);
            drawer.AddDrawerListener(drawerToggle);
            drawerToggle.SyncState();

            navigationView = FindViewById<NavigationView>(Resource.Id.navigation_view);
            navigationView.SetNavigationItemSelectedListener(this);

            var initialMenuItem = navigationView.Menu.FindItem(Resource.Id.nav_documents);
            initialMenuItem.SetChecked(true);
            OnNavigationItemSelected(initialMenuItem);

            Task.Run(async () =>
            {
                var authenticator = AuthenticatorFactory.Create();
                var ci = await authenticator.GetConnectionInfoAsync();
                var ss = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);
                return new object[] { ci, ss };
            }).ContinueWith(t =>
            {
                var ci = t.Result[0] as ConnectionInfo;
                var ss = t.Result[1] as SystemSettings;

                var headerTitle = FindViewById<AppCompatTextView>(Resource.Id.nav_header_title);
                var headerSubtitle = FindViewById<AppCompatTextView>(Resource.Id.nav_header_subtitle);

                headerTitle.Text = $"{ss?.UserInfo?.User?.FirstName} {ss?.UserInfo?.User?.LastName}";
                headerSubtitle.Text = $"{ci?.Username}@{ci?.Hostname}:{ci?.Port}";
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void OnResume()
        {
            base.OnResume();

            CrashManager.Register(this, "137e2a4fb6384cb3a51de617dd2f5999", new CustomCrashManagerListener());
            FeedbackManager.Register(this, "137e2a4fb6384cb3a51de617dd2f5999", new CustomFeedbackManagerLister());
            CrashManager.ResetAlwaysSend(new Java.Lang.Ref.WeakReference(this));
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

                var foldersListFragment = new FoldersListFragment
                {
                    Text = menuItem.ItemId,
                    Arguments = Intent.Extras
                };

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, foldersListFragment);
                ft.Commit();
            }

            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
    }
}

