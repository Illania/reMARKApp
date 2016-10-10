//
// Project: 
// File: CommentsActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity]
    public class CommentsActivity : BaseAppCompatActivity
    {
        public const string CommentsIntentKey = "Comments_20c8514c-b644-47db-842f-f2df4204d93a";

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
                var comments = SerializationUtils.Deserialize<List<Comment>>(Intent.Extras.GetString(CommentsIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                var cf = new CommentsFragment
                {
                    Comments = comments,
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
    }
}
