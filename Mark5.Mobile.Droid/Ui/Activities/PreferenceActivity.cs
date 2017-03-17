//
// Project: Mark5.Mobile.Droid
// File: PreferenceActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity]
    public class PreferenceActivity : BaseAppCompatActivity
    {

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PreferenceFragment)}...");

            SetTitle(Resource.String.settings);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var ft = SupportFragmentManager.BeginTransaction();
                var paf = new PreferenceFragment();
                ft.Replace(Resource.Id.fragment_container, paf, nameof(PreferenceFragment));
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PreferenceFragment)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PreferenceFragment)}");
            }
        }
    }
}
