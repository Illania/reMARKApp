//
// Project: 
// File: CommentsActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class CommentsListActivity : BaseAppCompatActivity
    {

        public const string EntityIntentKey = "EntityIntent_20c8514c-b644-47db-842f-f2df4204d93a";
        public const string CommentsResultKey = "CommentsResult_593d8c70-d45c-425e-8e36-7389e3cc0c62";

        CommentsListFragment cf;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(CommentsListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.document);
            SetContentView(Resource.Layout.base_layout);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var businessEntity = SerializationUtils.Deserialize<BusinessEntity>(Intent.Extras.GetString(EntityIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                cf = new CommentsListFragment
                {
                    Entity = businessEntity
                };
                ft.Replace(Resource.Id.fragment_container, cf, cf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(CommentsListActivity)}");
            }
            else
            {
                cf = (CommentsListFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(CommentsListActivity)}");
            }
        }

        public override void OnBackPressed()
        {
            if (cf != null)
            {
                var intent = new Intent();
                intent.PutExtra(CommentsResultKey, SerializationUtils.Serialize(cf.Comments));
                SetResult(Result.Ok, intent);
            }

            base.OnBackPressed();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}
