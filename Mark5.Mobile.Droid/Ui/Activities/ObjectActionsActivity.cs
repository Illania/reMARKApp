//
// Project: Mark5.Mobile.Droid
// File: ObjectActionsActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
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
        public const string BusinessEntityTypeIntentKey = "BusinessEntityType_5763e14a-b99d-4bdd-9cec-f54cfaf17ae0";

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
                var type = SerializationUtils.Deserialize<Type>(Intent.Extras.GetString(BusinessEntityTypeIntentKey));
                var be = SerializationUtils.Deserialize(Intent.Extras.GetString(BusinessEntityIntentKey), type) as IBusinessEntity;
                var ft = SupportFragmentManager.BeginTransaction();
                var dlf = new ObjectActionsFragment
                {
                    BusinessEntity = be
                };
                ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ObjectActionsActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ObjectActionsActivity)}");
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                OnBackPressed();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}

