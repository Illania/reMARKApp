//
// Project: 
// File: CommentsActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.Content;
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
    public class CommentsListActivity : BaseAppCompatActivity
    {
        public const string EntityIntentKey = "EntityIntent_20c8514c-b644-47db-842f-f2df4204d93a";
        public const string CommentsResultKey = "CommentsResult_593d8c70-d45c-425e-8e36-7389e3cc0c62";

        string fragmentTagKey = "fragmentTagKey";

        CommentsListFragment cf;
        string fragmentTag;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(CommentsListActivity)}...");

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
                    Entity = businessEntity,
                };
                fragmentTag = cf.GenerateTag();
                ft.Replace(Resource.Id.fragment_container, cf, fragmentTag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(CommentsListActivity)}");
            }
            else
            {
                fragmentTag = savedInstanceState.GetString(fragmentTagKey);
                cf = SupportFragmentManager.FindFragmentByTag(fragmentTag) as CommentsListFragment;
                CommonConfig.Logger.Info($"Restored {nameof(CommentsListActivity)}");
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(fragmentTagKey, fragmentTag);
            base.OnSaveInstanceState(outState);
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

        public override void OnBackPressed()
        {
            var intent = new Intent();
            intent.PutExtra(CommentsResultKey, SerializationUtils.Serialize(cf.Comments));
            SetResult(Result.Ok, intent);
            base.OnBackPressed();
        }
    }
}
