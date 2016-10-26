//
// Project: 
// File: CategoriesListActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
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

namespace Mark5.Mobile.Droid
{
    [Activity]
    public class CategoriesListActivity : BaseAppCompatActivity
    {
        public const string BusinessEntityPreviewIntentKey = "BusinessEntityPreview_43dc8df1-dc88-4e39-81d6-59ea495c35ff";

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(CategoriesListActivity)}...");

            SetTitle(Resource.String.document);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var bep = SerializationUtils.Deserialize<BusinessEntityPreview>(Intent.Extras.GetString(BusinessEntityPreviewIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                var clf = new CategoriesListFragment
                {
                    BusinessEntityPreview = bep
                };
                ft.Replace(Resource.Id.fragment_container, clf, clf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(CategoriesListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(CategoriesListActivity)}");
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
