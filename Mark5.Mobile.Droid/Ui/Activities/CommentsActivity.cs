//
// Project: 
// File: CommentsActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
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
    public class CommentsActivity : BaseAppCompatActivity
    {
        public const string EntityIntentKey = "Comments_20c8514c-b644-47db-842f-f2df4204d93a";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(CommentsActivity)}...");

            SetTitle(Resource.String.document);
            SetContentView(Resource.Layout.base_layout);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var businessEntity = SerializationUtils.Deserialize<BusinessEntity>(Intent.Extras.GetString(EntityIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                var cf = new CommentsFragment
                {
                    Entity = businessEntity,
                };
                ft.Replace(Resource.Id.fragment_container, cf, cf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(CommentsActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(CommentsActivity)}");
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
