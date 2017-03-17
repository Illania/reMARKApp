//
// Project: Mark5.Mobile.Droid
// File: CopyToUserWorktrayActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity]
    public class CopyToUserWorktrayActivity : BaseAppCompatActivity
    {

        const string BusinessEntitiesIntentKey = "BusinessEntities_79eb003f-6e04-4835-8820-fdd4e53a013b";

        public static Intent CreateIntent(Context context, List<IBusinessEntity> businessEntities)
        {
            var i = new Intent(context, typeof(CopyToUserWorktrayActivity));
            i.PutExtra(BusinessEntitiesIntentKey, SerializationUtils.Serialize(businessEntities));
            return i;
        }

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(CopyToUserWorktrayActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.select_users);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var be = SerializationUtils.Deserialize<List<IBusinessEntity>>(Intent.Extras.GetString(BusinessEntitiesIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                var dlf = new CopyToUserWorktrayFragment
                {
                    BusinessEntities = be,
                    CloseRequest = OnBackPressed
                };
                ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(CopyToUserWorktrayActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(CopyToUserWorktrayActivity)}");
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}
