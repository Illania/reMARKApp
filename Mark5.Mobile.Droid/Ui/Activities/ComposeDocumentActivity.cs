//
// Project: Mark5.Mobile.Droid
// File: ComposeDocumentActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity]
    public class ComposeDocumentActivity : AppCompatActivity
    {
        Toolbar toolbar;
        ComposeDocumentFragment cdf;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ComposeDocumentActivity)}...");

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var ft = SupportFragmentManager.BeginTransaction();
                cdf = new ComposeDocumentFragment { };
                ft.Replace(Resource.Id.fragment_container, cdf, cdf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ComposeDocumentActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ComposeDocumentActivity)}");
            }
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
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
