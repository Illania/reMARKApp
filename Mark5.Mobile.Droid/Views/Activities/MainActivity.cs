//
// Project: Mark5.Mobile.Droid
// File: MainActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;
using UK.CO.Chrisjenx.Calligraphy;

namespace Mark5.Mobile.Droid.Views.Activity
{

    [Activity]
    public class MainActivity : AppCompatActivity
    {

        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(CalligraphyContextWrapper.Wrap(@base));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetTitle(Resource.String.app_name);
            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            var navigationView = FindViewById<NavigationView>(Resource.Id.navigation_view);
            navigationView.NavigationItemSelected += (sender, e) =>
            {
                drawer.CloseDrawer(GravityCompat.Start);
            };

            Task.Run(async () =>
            {
                var authenticator = AuthenticatorFactory.Create();
                var ci = await authenticator.GetConnectionInfoAsync();
                var ss = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);

                Ui.RunOnUiThread(this, () =>
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
            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}

