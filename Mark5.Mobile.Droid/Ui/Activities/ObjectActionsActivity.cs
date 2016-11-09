//
// Project: Mark5.Mobile.Droid
// File: ObjectActionsActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
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
    public class ObjectActionsActivity : BaseAppCompatActivity
    {

        public const string BusinessEntityIntentKey = "BusinessEntity_ef8f3886-1478-4b4c-8bdb-7a6188035674";

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ObjectActionsActivity)}...");

            SetTitle(Resource.String.actions);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var be = SerializationUtils.Deserialize<IBusinessEntity>(Intent.Extras.GetString(BusinessEntityIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                var oaf = new ObjectActionsFragment
                {
                    BusinessEntity = be
                };
                ft.Replace(Resource.Id.fragment_container, oaf, oaf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ObjectActionsActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ObjectActionsActivity)}");
            }
        }
    }
}

