//
// Project: Mark5.Mobile.Droid
// File: PickCategoriesListActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid
{

    [Activity]
    public class PickCategoriesListActivity : BaseAppCompatActivity
    {

        public const string ObjectTypeIntentKey = "ObjectType_eede2fd6-3ad7-4503-adec-fdeb5ac44584";
        public const string PreselectedCategoryIdsIntentKey = "PreselectedCategoryIdsIntentKey_41340fa3-c8e3-4090-80ee-49ba5b062d67";
        public const string CategoriesResultKey = "CategoriesResult_36d29e7f-7336-42d6-9162-95178f8fec87";

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PickCategoriesListActivity)}...");

            SetTitle(Resource.String.categories);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var ot = (ObjectType)Intent.Extras.GetInt(ObjectTypeIntentKey);
                var pci = Intent.Extras.GetIntArray(PreselectedCategoryIdsIntentKey);
                var ft = SupportFragmentManager.BeginTransaction();
                var pclf = new PickCategoriesListFragment
                {
                    ObjectType = ot,
                    PreselectedCategoryIds = pci,
                    CloseRequest = categories =>
                    {
                        var intent = new Intent();
                        intent.PutExtra(CategoriesResultKey, SerializationUtils.Serialize(categories));
                        SetResult(Result.Ok, intent);
                        OnBackPressed();
                    }
                };
                ft.Replace(Resource.Id.fragment_container, pclf, pclf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PickCategoriesListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PickCategoriesListActivity)}");
            }
        }
    }
}
