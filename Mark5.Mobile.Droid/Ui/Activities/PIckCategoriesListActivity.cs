using System;
using System.Collections.Generic;
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

namespace Mark5.Mobile.Droid
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class PickCategoriesListActivity : BaseAppCompatActivity
    {
        public const string ObjectTypeIntentKey = "ObjectType_eede2fd6-3ad7-4503-adec-fdeb5ac44584";
        public const string PreselectedCategoryIdsIntentKey = "PreselectedCategoryIdsIntentKey_41340fa3-c8e3-4090-80ee-49ba5b062d67";
        public const string CategoriesResultKey = "CategoriesResult_36d29e7f-7336-42d6-9162-95178f8fec87";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(PickCategoriesListActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PickCategoriesListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.categories);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var ot = (ObjectType) Intent.Extras.GetInt(ObjectTypeIntentKey);
                var pci = Intent.Extras.GetIntArray(PreselectedCategoryIdsIntentKey);
                var ft = SupportFragmentManager.BeginTransaction();
                var pclf = new PickCategoriesListFragment(ot, pci, new CategoriesCloseRequest(categories => 
                {
                    var intent = new Intent();
                    intent.PutExtra(CategoriesResultKey, Serializer.Serialize(categories));
                    SetResult(Result.Ok, intent);
                    OnBackPressed();
                }));
                ft.Replace(Resource.Id.fragment_container, pclf, pclf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PickCategoriesListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PickCategoriesListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}