//
// Project: 
// File: ContactViewActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Views.Common;
using Mark5.Mobile.Droid.Views.Fragments;

namespace Mark5.Mobile.Droid.Views.Activities
{
    [Activity]
    public class ContactViewActivity : BaseAppCompatActivity
    {
        public const string ContactPreviewIntentKey = "ContactPreview_0da27d12-4d29-4f44-8dbf-2e28d7f93aae";

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ContactViewActivity)}...");

            SetTitle(Resource.String.contact);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var contactPreview = SerializationUtils.Deserialize<ContactPreview>(Intent.Extras.GetString(ContactPreviewIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                var dlf = new ContactViewFragment
                {
                    ContactPreview = contactPreview
                };
                ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ContactViewActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ContactViewActivity)}");
            }
        }
    }
}
